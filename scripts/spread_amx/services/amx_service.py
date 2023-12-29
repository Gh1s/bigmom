import datetime
import logging
import pysftp
from scripts.config.config import Kafka_Config
from scripts.spread_amx.config.config import Job_Config, Ace_Bdd_Config
from scripts.spread_amx.config.config import logger, yaml_config
from scripts.handles.producer_handles import handle_producer
from scripts.config.config import Kafka_Status


kafka_config = Kafka_Config(yaml_config)
kafka_producer_amex_config = kafka_config.producers.maj_amex_response
job_config = Job_Config(yaml_config)
ace_bdd_config = Ace_Bdd_Config(yaml_config)


def handle_messages_kafka(msg):
    if msg["application_code"] == "AMEX" or msg["application_code"] == "AMX":
        logger.debug("Traitement Amex en cours")
        date_datetime_amex = datetime.datetime.strptime(msg["heure_tlc"], '%Y-%m-%d' + 'T' + '%H:%M:%S%z')
        msg["heure_tlc"] = date_datetime_amex.strftime('%H%M')

        if len(msg["no_site_tpe"]) == 3 or len(msg["no_site_tpe"]) == 2:
            if len(msg["no_site_tpe"]) == 3:
                msg["no_site_tpe"] = msg["no_site_tpe"][1:3]

            elif len(msg["no_site_tpe"]) == 2:
                msg["no_site_tpe"] = msg["no_site_tpe"]
            send_tlc_hour_ace(msg["no_contrat"], msg["no_site_tpe"], msg["heure_tlc"])
            handle_producer(kafka_producer_amex_config.topic, job_config.job_property,
                            Kafka_Status.UPDATED.value, None, msg["trace_identifier"],
                            kafka_producer_amex_config.bootstrap_servers, logger)
            heure_amex_string = modif_heures_amex(msg["heure_tlc"])
            log_date_committed(heure_amex_string)
            return Kafka_Status.UPDATED.value

        else:
            logger.error("#####  Erreur le no de TPE doit etre sur 2 ou 3 chiffres")
            handle_producer(kafka_producer_amex_config.topic, job_config.job_property,
                            Kafka_Status.ERROR.value, f'Invalid value for field no_site_tpe {msg["no_site_tpe"]}', 
                            msg["trace_identifier"], kafka_producer_amex_config.bootstrap_servers, logger)
            return Kafka_Status.ERROR.value

    else:
        logger.info("Cette notification ne concerne pas un client AMEX")
        handle_producer(kafka_producer_amex_config.topic, job_config.job_property,
                        Kafka_Status.IGNORED.value, None, msg["trace_identifier"],
                        kafka_producer_amex_config.bootstrap_servers, logger)
        return Kafka_Status.IGNORED.value


def send_tlc_hour_ace(no_contrat, num_tpe, tlc_hour):
    logger.info("Récupération des paramètres pour la connexion à la BDD ACE. ")
    hostname = ace_bdd_config.hostname
    username = ace_bdd_config.username
    password = ace_bdd_config.password
    oracle_ace_env = ace_bdd_config.oracle_env

    cnopts = pysftp.CnOpts()
    cnopts.hostkeys = None
    command = oracle_ace_env + "/csb/bin/update_amex_ace_database.pl " + no_contrat + " " + num_tpe + " " + tlc_hour
    with pysftp.Connection(hostname, username=username, password=password, cnopts=cnopts) as sftp:
        logger.info("Envoi de la commande de mise a l'heure du commerçant: " + str(no_contrat) + "numéro de TPE: " + \
                    str(num_tpe) + "heure de télécollecte: " + tlc_hour)
        retour = sftp.execute(command)
        for line in retour:
            logger.info(line)
    sftp.close()

    return None


def log_date_committed(date):
    logger.info("Mise à jour du TPE ok, la nouvelle heure de télecollecte est: " + str(date))


def modif_heures_amex(self):
    heure_datetime = datetime.datetime.strptime(self, '%H%M')
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
