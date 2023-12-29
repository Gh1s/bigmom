import datetime
import cx_Oracle
import logging
import json

from kafka import KafkaProducer, KafkaConsumer
from scripts.spread_pwc.config.config import Oracle_Config, Pwc_Config, Job_Config, yaml_config, logger
from scripts.config.config import Kafka_Status
from scripts.handles.producer_handles import handle_producer
from scripts.config.config import Kafka_Config


oracle_config = Oracle_Config(yaml_config)
kafka_config = Kafka_Config(yaml_config)
kafka_producer_pwc_config = kafka_config.producers.maj_powercard_response
kafka_producer_pwc_data_config = kafka_config.producers.data_powercard_response
pwc_config = Pwc_Config(yaml_config)
job_config = Job_Config(yaml_config)


def handle_data_request(msg):
    if msg["job"] == job_config.job_pwc_property:
        logger.debug("Récupération de l'IDSA en cours pour une application ")
        idsa = None
        status = None
        try:
            idsa = get_idsa_from_pwc(msg["params"]['no_contrat'], msg["params"]['no_serie_tpe'],
                                     msg["params"]['no_site_tpe'], msg["params"]['application_code'])
            status = Kafka_Status.HANDLED.value
        except Exception as e:
            logger.error(e)
            status = Kafka_Status.ERROR.value
        finally:
            send_idsa_kafka(msg, idsa)
            return status


def get_idsa_from_pwc(numero_contrat, numero_serie, numero_site, application_code):
    profiles = pwc_config.profiles[application_code.lower()]
    query = "SELECT t.TERMINAL_POS_NUMBER FROM TERMINAL_POS t \
             JOIN MER_ACCEPTOR_POINT m ON t.OUTLET_NUMBER = m.MER_ACCEPTOR_POINT_ID \
             WHERE m.EXTERNAL_ID_1  = '" + numero_contrat + "' AND t.SERIAL_NUMBER = SUBSTR('" + numero_serie + "',-8) \
             AND t.EQUIPMENT_FEE_CODE = LPAD('" + numero_site + "',3,'0') AND t.POS_PROFILE_CODE IN (" + ",".join(f"'{x}'" for x in profiles) + ")"
    logger.info("Connexion a la base de données Oracle")
    dsn_tns = cx_Oracle.makedsn(oracle_config.server, oracle_config.port,
                                oracle_config.service)
    with cx_Oracle.connect(user=oracle_config.user, password=oracle_config.password,
                           dsn=dsn_tns) as conn:
        logger.info("Récupération de l'IDSA")
        logger.debug("Requête : {0}".format(query))
        with conn.cursor() as cursor:
            cursor.execute(query)
            row = cursor.fetchone()
            logger.info("Déconnexion de la base Powercard")
            cursor.close()
            if row != None:
                logger.info("IDSA = " + str(row[0]))
                return row[0]
            else:
                logger.info("IDSA introuvable")
                return None


def send_idsa_kafka(msg, idsa):
    data = {}
    s_msg = {}
    data["idsa"] = idsa
    s_msg["job"] = job_config.job_pwc_property
    s_msg["trace_identifier"] = msg["trace_identifier"]
    s_msg["params"] = msg["params"]
    s_msg["data"] = data
    logger.info("Envoi des éléments suivants vers kafka " + str(s_msg))
    producer = KafkaProducer(value_serializer=lambda m: json.dumps(m, ensure_ascii=False).encode('utf-8'),
                             bootstrap_servers=kafka_producer_pwc_data_config.bootstrap_servers)
    try:
        producer.send(kafka_producer_pwc_data_config.topic, s_msg)
        producer.flush()

    except Exception as e:
        logger.error(e)


def update_applis_bancaire_pwc(no_contrat, numero_serie, numero_site, heure_telecollecte, application_code, code_banque):
    logger.info("Connexion a la base de données Oracle")
    dsn_tns = cx_Oracle.makedsn(oracle_config.server, oracle_config.port,
                                oracle_config.service)
    with cx_Oracle.connect(user=oracle_config.user, password=oracle_config.password,
                             dsn=dsn_tns) as conn:
        with conn.cursor() as cursor:
            def get_idsa():
                logger.info("Récupération de l'IDSA")
                profiles = pwc_config.profiles[application_code.lower()]
                query = "SELECT t.TERMINAL_POS_NUMBER, t.POS_PROFILE_CODE FROM TERMINAL_POS t \
                        JOIN MER_ACCEPTOR_POINT m ON t.OUTLET_NUMBER = m.MER_ACCEPTOR_POINT_ID \
                        WHERE m.EXTERNAL_ID_1  = '" + no_contrat + "' AND t.SERIAL_NUMBER = SUBSTR('" + numero_serie + "',-8) \
                        AND t.EQUIPMENT_FEE_CODE = LPAD('" + numero_site + "',3,'0') AND t.POS_PROFILE_CODE IN (" + ",".join(f"'{x}'" for x in profiles) + ") \
                        ORDER BY INSTALLATION_DATE DESC"
                logger.debug("Requête : {0}".format(query))
                cursor.execute(query)
                row = cursor.fetchone()
                if row == None:
                    raise ValueError("IDSA introuvable")
                return (row[0], row[1])
            def update_dial_hour():
                logger.info("Mise à jour de l'heure de télécollecte bancaire.")
                cmd = "UPDATE TERMINAL_POS SET DIAL_HOUR = TO_DATE('" + heure_telecollecte + "','DD/MM/YY HH24:MI:SS') \
                    WHERE TERMINAL_POS_NUMBER = '" + terminal_pos_number + "'"
                logger.debug("Requête : {0}".format(cmd))
                cursor.execute(cmd)
                if cursor.rowcount == 0:
                    raise ValueError("Aucune ligne n'a été modifiée par la requête : {0}".format(cmd))
            def dld_mng_exists():
                logger.info(
                    "Vérification de l'existence de la ligne dans POS_CB2A_DLD_MNG.")
                query = "SELECT CASE WHEN EXISTS (SELECT 1 FROM POS_CB2A_DLD_MNG pcadm WHERE pcadm.BANK_CODE = '" + code_banque + "' \
                        AND pcadm.TERMINAL_NBR = '" + terminal_pos_number + "' AND pcadm.DWNLD_TYPE = '" + pwc_config.forced_dld_type + "') \
                        THEN 'Y' ELSE 'N' END AS REC_EXISTS FROM DUAL"
                logger.debug("Requête : {0}".format(query))
                cursor.execute(query)
                return cursor.fetchone()[0] == "Y"
            def update_dld_mng():
                logger.info("Mise à jour de la table POS_CB2A_DLD_MNG.")
                cmd = "UPDATE POS_CB2A_DLD_MNG SET PROCESSING_STATUS = '0', FORCE_FLAG = 'Y', \
                    START_VAL_DATE = TRUNC(TO_DATE('" + heure_telecollecte + "', 'DD/MM/YY HH24:MI:SS')), \
                    USER_MODIF = 'USD_PCARD', DATE_MODIF = SYSDATE \
                    WHERE BANK_CODE = '" + code_banque + "' AND TERMINAL_NBR = '" + terminal_pos_number + "' \
                    AND DWNLD_TYPE = '" + pwc_config.forced_dld_type + "'"
                logger.debug("Requête : {0}".format(cmd))
                cursor.execute(cmd)
                if cursor.rowcount == 0:
                    raise ValueError("Aucune ligne n'a été modifiée par la requête : {0}".format(cmd))
            def insert_dld_mng():
                logger.info(
                    "Récupération du FILE_VERSION dans la table POS_CB2A_TELECOM.")
                query = "SELECT MAX(FILE_VERSION) FROM POS_CB2A_TELECOM pcat WHERE pcat.BANK_CODE = '" + code_banque + "' \
                        AND pcat.POS_PROFILE_CODE = '" + terminal_pos_profile_code + "' AND pcat.TRANSFER_TYPE = 'U'"
                logger.debug("Requête : {0}".format(query))
                cursor.execute(query)
                file_version = cursor.fetchone()[0]
                logger.info("Insertion dans la table POS_CB2A_DLD_MNG.")
                cmd = "INSERT INTO POS_CB2A_DLD_MNG (BANK_CODE, TERMINAL_NBR, POS_PROFILE_CODE, DWNLD_TYPE, FILE_VERSION, TERMINAL_VERSION, \
                    NBR_OF_MSGS, WND_ACKNLDG, FILE_ACTION, DIGEST_MNG, PROCESSING_STATUS, FORCE_FLAG, START_VAL_DATE, USER_CREATE, DATE_CREATE) \
                    VALUES ('" + code_banque + "', '" + terminal_pos_number + "', '" + terminal_pos_profile_code + "', '" + pwc_config.forced_dld_type + "', \
                    '" + file_version + "', '" + pwc_config.default_terminal_version + "', 1, 1, '2', '0', '0', 'Y', \
                    TRUNC(TO_DATE('" + heure_telecollecte + "', 'DD/MM/YY HH24:MI:SS')), 'USR_PCARD', SYSDATE)"
                logger.debug("Requête : {0}".format(cmd))
                cursor.execute(cmd)
                if cursor.rowcount == 0:
                    raise ValueError("Aucune ligne n'a été modifiée par la requête : {0}".format(cmd))

            terminal_pos_number, terminal_pos_profile_code = get_idsa()
            logger.debug("IDSA = " + str(terminal_pos_number))
            logger.debug("POS profile code = " + str(terminal_pos_profile_code))

            update_dial_hour()

            if (dld_mng_exists()):
                update_dld_mng()
            else:
                insert_dld_mng()

            logger.info("Commit des requêtes")
            conn.commit()
            logger.info("Déconnexion de la base Powercard")


def update_applis_privative_pwc(code_banque, numero_site, numero_bleu, heure_telecollecte):
    requete = "update TERMINAL_POS_CBPR set hour_call_ct = '" + heure_telecollecte + "'" + \
        " where BANK_CODE = '" + code_banque + "' and TERMINAL_NUMBER = LPAD(LTRIM('" \
        + numero_site + "', '0'), 2, '0') and SITE_NUMBER = '" + numero_bleu + "'"
    logger.info("Connexion a la base de données Oracle")
    dsn_tns = cx_Oracle.makedsn(oracle_config.server, oracle_config.port,
                                oracle_config.service)
    with cx_Oracle.connect(user=oracle_config.user, password=oracle_config.password,
                           dsn=dsn_tns) as conn:
        logger.info("Mise à jour à jour de l'heure de télécollecte privative.")
        logger.info("Requête : {0}".format(requete))
        with conn.cursor() as c:
            c.execute(requete)
            if c.rowcount >= 1:
                logger.info("Mise à jour OK.")
            else:
                logger.error("Mise à jour non OK.")
                raise ValueError("Aucune ligne n'a été modifiée par la requête : {0}".format(requete))
            logger.info("Commit de la requête")
            conn.commit()
            logger.info("Déconnexion de la base Powercard")


def handle_spread_message(msg):
    if msg["application_code"] in pwc_config.bancaire_apps:
        logger.info("Traitement bancaire en cours")
        msg["heure_tlc"] = transformation_dates_emv(msg["heure_tlc"])
        try:
            update_applis_bancaire_pwc(msg["no_contrat"], msg["no_serie_tpe"], msg["no_site_tpe"],
                                       msg["heure_tlc"], msg["application_code"], msg["ef"])
        except Exception as e:
            logger.error(e)
            handle_producer(kafka_producer_pwc_config.topic, job_config.job_pwc_property, Kafka_Status.ERROR.value,
                            str(e), msg["trace_identifier"], kafka_producer_pwc_config.bootstrap_servers, logger)
            return Kafka_Status.ERROR.value
        handle_producer(kafka_producer_pwc_config.topic, job_config.job_pwc_property, Kafka_Status.UPDATED.value,
                        None, msg["trace_identifier"], kafka_producer_pwc_config.bootstrap_servers, logger)
        heure_emv_string = modif_heures_emv(msg["heure_tlc"])
        log_date_committed(heure_emv_string)
        return Kafka_Status.UPDATED.value

    elif msg["application_code"] in pwc_config.privative_apps:
        logger.info("Traitement privatif en cours")
        msg["heure_tlc"] = transformation_dates_priv(msg["heure_tlc"])
        try:
            update_applis_privative_pwc(
                msg["ef"], msg["no_site_tpe"], msg["no_contrat"], msg["heure_tlc"])
        except Exception as e:
            logger.error(e)
            handle_producer(kafka_producer_pwc_config.topic, job_config.job_pwc_property, Kafka_Status.ERROR.value,
                            str(e), msg["trace_identifier"], kafka_producer_pwc_config.bootstrap_servers, logger)
            return Kafka_Status.ERROR.value
        handle_producer(kafka_producer_pwc_config.topic, job_config.job_pwc_property, Kafka_Status.UPDATED.value,
                        None, msg["trace_identifier"], kafka_producer_pwc_config.bootstrap_servers, logger)
        log_date_committed(modif_heures_prv(msg["heure_tlc"]))
        return Kafka_Status.UPDATED.value

    else:
        logger.info("Application non supportée")
        handle_producer(kafka_producer_pwc_config.topic, job_config.job_pwc_property, Kafka_Status.IGNORED.value,
                        None, msg["trace_identifier"], kafka_producer_pwc_config.bootstrap_servers, logger)
        return Kafka_Status.IGNORED.value


def transformation_dates_emv(date):
    date_datetime_emv = datetime.datetime.strptime(
        date, '%Y-%m-%d' + 'T' + '%H:%M:%S%z')
    date_string_emv = date_datetime_emv.strftime('%d/%m/%y %H:%M:%S')
    return date_string_emv


def transformation_dates_priv(date):
    date_datetime_priv = datetime.datetime.strptime(
        date, '%Y-%m-%d' + 'T' + '%H:%M:%S%z')
    date_string_priv = date_datetime_priv.strftime('%H%M')
    return date_string_priv


def log_date_committed(date):
    logger.info(
        "Mise à jour du TPE ok, la nouvelle heure de télecollecte est: " + str(date))


def modif_heures_emv(date):
    heure_datetime = datetime.datetime.strptime(date, '%d/%m/%y %H:%M:%S')
    heure_string = heure_datetime.strftime('%H' + 'h' + '%M')
    return heure_string


def modif_heures_prv(date):
    heure_datetime = datetime.datetime.strptime(date, '%H%M')
    heure_string = heure_datetime.strftime('%H' + 'h' + '%M')
    return heure_string


def log_mode_debug():
    if yaml_config['debug_log']:
        logging.basicConfig(level=logging.DEBUG, format="%(asctime)s [%(levelname)s] %(name)-12s - %(message)s",
                            handlers=[
                                # logging.FileHandler(Config().yaml_config["log_dir"] + "log_debug.log"),
                                logging.StreamHandler()
                            ])
    else:
        logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(name)-12s - %(message)s",
                            handlers=[
                                # logging.FileHandler(Config().yaml_config["log_dir"] + "log_info.log"),
                                logging.StreamHandler()
                            ])
