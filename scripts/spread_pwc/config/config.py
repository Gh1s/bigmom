import logging
import yaml
import os


logger = logging.getLogger('telecollecte-processing')
fichier_yaml = open(os.getenv("CONFIG_PATH") or 'scripts/spread_pwc/config/config.yml')
yaml_config = yaml.load(fichier_yaml, Loader=yaml.FullLoader)


class Oracle_Config:
    def __init__(self, yaml_config):
        self.server = yaml_config['oracle']['server']
        self.port = yaml_config['oracle']['port']
        self.user = yaml_config['oracle']['user']
        self.service = yaml_config['oracle']['service']
        self.password = yaml_config['oracle']['password']

class Pwc_Config:
    def __init__(self, yaml_config):
        self.bancaire_apps = yaml_config['pwc']['bancaire_apps']
        self.privative_apps = yaml_config['pwc']['privative_apps']
        self.forced_dld_type = yaml_config['pwc']['forced_dld_type']
        self.default_terminal_version = yaml_config['pwc']['default_terminal_version']
        self.profiles = yaml_config['pwc']['profiles']

class TPE_Config:
    def __init__(self, message):
        self.trace_identifier = message['trace_identifier']
        self.id_commercant = message['id_commercant']
        self.no_serie_tpe = message['no_serie_tpe']
        self.no_site_tpe = message['no_site_tpe']
        self.code_application = message['code_application']
        self.heure_tlc = message['heure_tlc']
        self.banque = message['banque']


class Job_Config:
    def __init__(self, yaml_config):
        self.job_pwc_property = yaml_config['job_properties']['job_pwc_property']
