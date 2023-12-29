import logging
import yaml
import os

#from config.config import Kafka_Config

logger = logging.getLogger("collector_config")
CONFIG_PATH = os.getenv("CONFIG_PATH") or "C:\\Users\\grandg\\Documents\\Ghis\\programming\\projects\\bigmom\\scripts\\collector\\config\\config.yml"


class Collector_Config:
    def __init__(self, yaml_config):
        self.commerce_file = yaml_config['collector']['json']['files']['commerce']
        self.contrat_file = yaml_config['collector']['json']['files']['contrat']
        self.tpe_file = yaml_config['collector']['json']['files']['tpe']
        self.nb_process = yaml_config['collector']['nb_process']
        self.exclusion_list_type_tpe = yaml_config['collector']['exclusion_list_type_tpe']
        logger.debug("Collector_Config loaded")


class Config:
    # Singleton
    __instance = None
    kafka_config = None
    collector_config = None

    def __new__(cls):
        if Config.__instance is None:
            logger.info("New configuration")
            Config.__instance = object.__new__(cls)
            logger.info("__init__")
            Config.__instance.read_config_file()
            Config.__instance.apply_config()
            logger.info("EO __init__")
        return Config.__instance

    def apply_config(self):
        logger.info("apply_config")
        if self.yaml_config['debug_log']:
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
        self.collector_config = Collector_Config(self.yaml_config)
        #self.kafka_config = Kafka_Config(self.yaml_config).producers.integration_commerce_request

    def read_config_file(self):
        logger.info("read_config_file" + CONFIG_PATH)
        with open(CONFIG_PATH, 'r', encoding='utf8') as stream:
            try:
                self.yaml_config = yaml.safe_load(stream)
            except yaml.YAMLError as exc:
                logger.error("Le fichier de configuration yml est invalide.")
                logger.error(exc)
                exit(-1)