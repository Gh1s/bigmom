import csv
import datetime
from os import read
import queue
import threading
import pysftp

from scripts.config.config import Kafka_Config
from scripts.config.config import Kafka_Status
from scripts.spread_jcb.config.config import *
from scripts.handles.producer_handles import handle_producer

kafka_config = Kafka_Config(yaml_config)
kafka_producer_jcb_config = kafka_config.producers.maj_jcb_response
job_config = Job_Config(yaml_config)
ace_bdd_config = Ace_Bdd_Config(yaml_config)
osb_config = OSB_File(yaml_config)


def handle_messages_kafka(msg):
    if msg["application_code"] == "JCB":
        logger.info("Traitement JCB en cours")
        msg["heure_tlc"] = transformation_dates_jcb(msg["heure_tlc"])
        logger.info("Récupération du numéro d'adhérent. ")
        if not msg["no_contrat"] == None and not msg["no_contrat"] == "":
            filename = osb_config.repertoire + osb_config.nom_fichier + \
                datetime.datetime.now().strftime('%Y%m%d') + osb_config.extension_fichier
            write_row(filename, msg["no_contrat"], msg["no_site_tpe"], msg["heure_tlc"])
            handle_producer(kafka_producer_jcb_config.topic, job_config.job_property, Kafka_Status.UPDATED.value,
                            None, msg["trace_identifier"], kafka_producer_jcb_config.bootstrap_servers, logger)
            heure_jcb_string = modif_heures_jcb(msg["heure_tlc"])
            log_date_committed(heure_jcb_string)
            return Kafka_Status.UPDATED.value
        else:
            error = f"Le numéro de contrat n'est pas renseigné"
            logger.error(error)
            handle_producer(kafka_producer_jcb_config.topic, job_config.job_property, Kafka_Status.ERROR.value,
                            error, msg["trace_identifier"], kafka_producer_jcb_config.bootstrap_servers, logger)
            return Kafka_Status.ERROR.value
    else:
        logger.info("Cette notification ne concerne pas un client JCB")
        handle_producer(kafka_producer_jcb_config.topic, job_config.job_property, Kafka_Status.IGNORED.value,
                        None, msg["trace_identifier"], kafka_producer_jcb_config.bootstrap_servers, logger)
        return Kafka_Status.IGNORED.value


def transformation_dates_jcb(date):
    date_datetime_priv = datetime.datetime.strptime(date, '%Y-%m-%d' + 'T' + '%H:%M:%S%z')
    date_string_priv = date_datetime_priv.strftime('%H%M')
    return date_string_priv


def modif_heures_jcb(date):
    heure_datetime = datetime.datetime.strptime(date, '%H%M')
    heure_string = heure_datetime.strftime('%H' + 'h' + '%M')
    return heure_string

def write_row(file_name, num_adherent, no_site_tpe, heure_tlc):
    with open(file_name, "r+", newline='\n') as file:
        reader = csv.reader(file, delimiter=';')
        for row in reader:
            if row[0] == num_adherent and row[1] == no_site_tpe:
                logger.info(f"La ligne [{num_adherent},{no_site_tpe},{heure_tlc}] existe déjà dans le fichier {file_name}.")
                return
        logger.info(f"Ecriture de la ligne [{num_adherent},{no_site_tpe},{heure_tlc}]  dans le fichier {file_name}.")
        writer = csv.writer(file, delimiter=';')
        writer.writerow([num_adherent, no_site_tpe, heure_tlc])


def log_date_committed(date):
    logger.info("Mise à jour du TPE ok, la nouvelle heure de télecollecte est: " + str(date))


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
