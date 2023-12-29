import logging
import yaml
import os

from config.config import Kafka_Config

logger = logging.getLogger("collector_config")
CONFIG_PATH = os.getenv("CONFIG_PATH") or "scripts/data_generator/config/config.yml"


class Db_Config:
    def __init__(self, yaml_config):
        logger.debug("Loading Db config")
        self.host = yaml_config['db']['host']
        self.port = yaml_config['db']['port']
        self.database = yaml_config['db']['database']
        self.user = yaml_config['db']['user']
        self.password = yaml_config['db']['password']
        logger.debug("Db config loaded")


class Config:
    # Singleton
    __instance = None
    db_config = None
    kafka_config = None

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
        logger.info("Applying config")
        if self.yaml_config['debug_log']:
            logging.basicConfig(level=logging.DEBUG, format="%(asctime)s [%(levelname)s] %(name)-12s - %(message)s",
                                handlers=[logging.StreamHandler()])
        else:
            logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(name)-12s - %(message)s",
                                handlers=[logging.StreamHandler()])
        self.db_config = Db_Config(self.yaml_config)
        self.kafka_config = Kafka_Config(self.yaml_config)

    def read_config_file(self):
        logger.info("Reading config file: " + CONFIG_PATH)
        with open(CONFIG_PATH, 'r', encoding='utf8') as stream:
            try:
                self.yaml_config = yaml.safe_load(stream)
            except yaml.YAMLError as exc:
                logger.error("Invalid configuration file")
                logger.error(exc)
                exit(-1)
