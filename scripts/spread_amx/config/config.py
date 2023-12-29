import logging
import yaml
import os


logger = logging.getLogger('telecollecte-amex-processing')
fichier_yaml = open(os.getenv("CONFIG_PATH") or 'scripts/spread_amx/config/config.yml')
yaml_config = yaml.load(fichier_yaml, Loader=yaml.FullLoader)


class Ace_Connection:
    def __init__(self, yaml_config):
        self.host = yaml_config["connection_serveur_ace"]["host"]
        self.user = yaml_config["connection_serveur_ace"]["user"]


class Job_Config:
    def __init__(self, yaml_config):
        self.job_property = yaml_config['job_properties']['job_amex_property']


class Ace_Bdd_Config:
    def __init__(self, yaml_config):
        self.hostname = yaml_config['ace_bdd']['hostname']
        self.username = yaml_config['ace_bdd']['username']
        self.password = yaml_config['ace_bdd']['password']
        self.oracle_env = yaml_config['ace_bdd']['oracle_env']
        self.timeout = yaml_config['ace_bdd']['timeout']