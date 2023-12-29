import logging
import yaml
import os


logger = logging.getLogger('telecollecte-jcb-processing')
fichier_yaml = open(os.getenv("CONFIG_PATH") or 'scripts/spread_jcb/config/config.yml')
yaml_config = yaml.load(fichier_yaml, Loader=yaml.FullLoader)


class OSB_File:
    def __init__(self, yaml_config):
        self.repertoire = yaml_config["osb"]["repertoire"]
        self.nom_fichier = yaml_config["osb"]["nom_fichier"]
        self.extension_fichier = yaml_config["osb"]["extension_fichier"]


class Job_Config:
    def __init__(self, yaml_config):
        self.job_property = yaml_config['job_properties']['job_jcb_property']


class Ace_Bdd_Config:
    def __init__(self, yaml_config):
        self.hostname = yaml_config['ace_bdd']['hostname']
        self.username = yaml_config['ace_bdd']['username']
        self.password = yaml_config['ace_bdd']['password']
        self.oracle_env = yaml_config['ace_bdd']['oracle_env']
        self.timeout = yaml_config['ace_bdd']['timeout']