import json
import uuid
import logging

from datetime import datetime
from functools import partial
from kafka import KafkaProducer
from multiprocessing import Pool, Manager

from collector.models.commerce import Commerce
from collector.models.contrat import Contrat
from collector.models.mcc import Mcc
from collector.models.integration_commerce_request import Integration_Commerce_Request
from collector.models.tpe import Tpe
from collector.config.config import Config

conf = Config()

logger = logging.getLogger("Main")

PATH_TO_JSON_FILES = {
    "Commerce": conf.collector_config.commerce_file,
    "Contrat": conf.collector_config.contrat_file,
    "Tpe": conf.collector_config.tpe_file
}

JSON_MAPPING_KEY_TPE = {
    "no_serie": "TPE_SN",
    "no_site": "TPE_NumeroSite",
    "modele": "Modele",
    "statut": "Statut",
    "id_commerce": "IDCommerce",
    "type_connexion": "type_connexion"
}

JSON_MAPPING_KEY_CONTRAT = {
    "id_commerce": "IDCommerce",
    "date_debut": "date_debut",
    "code": "NomAppli",
    "date_fin": "date_fin",
    "no_contrat": "num_contrat"
}

JSON_MAPPING_KEY_MCC = {
    "code_mcc": "MCC",
    "range_start": None,
    "range_end": None
}

JSON_MAPPING_KEY_COMMERCE = {
    "id_commerce": "IDCommerce",
    "nom": "NomCommerce",
    "ef": "CodeEF",
    "date_affiliation": "date_debut",
    "date_resiliation": "date_fin"
}


def get_tpes_for_commerce(commerce_id, tpes):
    return [
        Tpe(no_serie=tpe[JSON_MAPPING_KEY_TPE["no_serie"]],
            no_site=tpe[JSON_MAPPING_KEY_TPE["no_site"]],
            statut=tpe[JSON_MAPPING_KEY_TPE["statut"]],
            modele=tpe[JSON_MAPPING_KEY_TPE["modele"]],
            type_connexion=tpe[JSON_MAPPING_KEY_TPE["type_connexion"]]
            )
        for tpe in tpes if tpe[JSON_MAPPING_KEY_TPE["id_commerce"]] == commerce_id
    ]


def convert_str_to_date_iso_str(date):
    if date is None:
        return None
    date_parsed = datetime.strptime(date, '%d-%m-%Y')
    date_iso = date_parsed.isoformat()
    return str(date_iso)


def get_contrats_for_commerce(commerce_id, contrats):
    return [
        Contrat(code=contrat[JSON_MAPPING_KEY_CONTRAT["code"]],
                date_debut=convert_str_to_date_iso_str(contrat[JSON_MAPPING_KEY_CONTRAT["date_debut"]]),
                date_fin=convert_str_to_date_iso_str(contrat[JSON_MAPPING_KEY_CONTRAT["date_fin"]]),
                no_contrat=contrat[JSON_MAPPING_KEY_CONTRAT["no_contrat"]]
                )
        for contrat in contrats if contrat[JSON_MAPPING_KEY_CONTRAT["id_commerce"]] == commerce_id
    ]


def filter_tpes_by_type(tpe_list):
    return [tpe for tpe in tpe_list if tpe["Type_Terminal"] not in conf.collector_config.exclusion_list_type_tpe]


def read_json_files(paths):
    result = []

    with open(paths["Commerce"], encoding="utf-8") as commerce_file, \
            open(paths["Contrat"], encoding="utf-8") as application_file, \
            open(paths["Tpe"], encoding="utf-8") as tpe_file:
        commerces = json.load(commerce_file)["Commerce"]
        contrats = json.load(application_file)["CommerceAppli"]
        tpes = filter_tpes_by_type(json.load(tpe_file)["CommerceTPE"])

        guid = uuid.uuid4()
        total = len(commerces)

        for index, commerce in enumerate(commerces):
            result.append(Integration_Commerce_Request(commerce=Commerce(
                identifiant=commerce[JSON_MAPPING_KEY_COMMERCE["id_commerce"]],
                mcc=Mcc(
                    code_mcc=commerce.get(JSON_MAPPING_KEY_MCC["code_mcc"])
                ),
                nom=commerce.get(JSON_MAPPING_KEY_COMMERCE["nom"]),
                ef=commerce.get(JSON_MAPPING_KEY_COMMERCE["ef"]),
                tpes=get_tpes_for_commerce(commerce[JSON_MAPPING_KEY_COMMERCE["id_commerce"]], tpes),
                contrats=get_contrats_for_commerce(commerce[JSON_MAPPING_KEY_COMMERCE["id_commerce"]], contrats),
                date_affiliation=convert_str_to_date_iso_str(
                    commerce.get(JSON_MAPPING_KEY_COMMERCE["date_affiliation"])),
                date_resiliation=convert_str_to_date_iso_str(
                    commerce.get(JSON_MAPPING_KEY_COMMERCE["date_resiliation"]))
            ),
                guid=guid,
                index=index,
                total=total
            ))
            logger.debug("N° {} Message to job".format(index))

    return result


def produce_into_kafka(message, producer):
    logger.debug("Sending message through kafka")
    producer.send(topic='bigmom.integration.commerce.requests.process', value=message.to_dict())


def do_job_multi(q, name):
    logger.debug("Process N°" + name + " : Starting")
    producer = KafkaProducer(
        bootstrap_servers=['localhost:9092'],
        value_serializer=lambda v: json.dumps(v).encode('utf-8'),
        max_request_size=5242880
    )
    logger.debug("Process N°" + name + " : initialized ")
    while True:
        message = q.get()
        logger.debug("Process N°" + name + " : message retrieved from queue")
        if message == "STOP":
            logger.debug("Process N°" + name + " : STOP")
            break
        produce_into_kafka(message, producer)
    producer.flush()
    logger.debug("Process N°" + name + " : Exiting ")

    return None


if __name__ == '__main__':
    logger.info("Loading files")
    data_object = read_json_files(PATH_TO_JSON_FILES)
    logger.info("Files aggregated and ready to send")

    manager = Manager()
    queue = manager.Queue()

    logger.info("Transferring data into internal queue for multiprocessing")
    for data in data_object:
        queue.put(data)
    for i in range(conf.collector_config.nb_process):
        queue.put("STOP")

    logger.info("Starting multiprocessing (number of process :{})".format(conf.collector_config.nb_process))
    pool = Pool(processes=conf.collector_config.nb_process)
    func = partial(do_job_multi, queue)
    res = pool.map(func, [str(i) for i in range(conf.collector_config.nb_process)])
