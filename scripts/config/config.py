from enum import Enum


class Kafka_Consumer:
    def __init__(self, yaml_config, name):
        self.bootstrap_servers = yaml_config["kafka"]["consumers"][name]['bootstrap_servers']
        self.group_id = yaml_config["kafka"]["consumers"][name]['group_id']
        self.auto_offset_reset = yaml_config["kafka"]["consumers"][name]['auto_offset_reset']
        self.auto_commit = yaml_config["kafka"]["consumers"][name]['auto_commit']
        self.auto_commit_interval_ms = yaml_config["kafka"]["consumers"][name]['auto_commit_interval_ms']
        self.max_poll_interval_ms = yaml_config["kafka"]["consumers"][name]['max_poll_interval_ms']
        self.topic = yaml_config["kafka"]["consumers"][name]['topic']


class Kafka_Producer:
    def __init__(self, yaml_config, name):
        self.bootstrap_servers = yaml_config["kafka"]["producers"][name]['bootstrap_servers']
        self.topic = yaml_config["kafka"]["producers"][name]['topic']
        self.max_request_size = yaml_config['kafka']['producers'][name]["max_request_size"] if "max_request_size" in yaml_config['kafka']['producers'][name] else None


class Kafka_Consumers_Config:
    def __init__(self, yaml_config):
        self.maj_powercard_request = Kafka_Consumer(yaml_config, "maj_powercard_request") if "maj_powercard_request" in yaml_config['kafka']['consumers'] else None
        self.data_powercard_request = Kafka_Consumer(yaml_config, "data_powercard_request") if "data_powercard_request" in yaml_config['kafka']['consumers'] else None
        self.maj_amex_request = Kafka_Consumer(yaml_config, "maj_amex_request") if "maj_amex_request" in  yaml_config['kafka']['consumers'] else None
        self.maj_jcb_request = Kafka_Consumer(yaml_config, "maj_jcb_request") if "maj_jcb_request" in yaml_config['kafka']['consumers'] else None


class Kafka_Producers_Config:
    def __init__(self, yaml_config):
        self.integration_commerce_request = Kafka_Producer(yaml_config, "integration_commerce_request") if "integration_commerce_request" in yaml_config['kafka']['producers'] else None
        self.tlc_index_request = Kafka_Producer(yaml_config, "tlc_index_request") if "tlc_index_request" in yaml_config['kafka']['producers'] else None
        self.maj_powercard_response = Kafka_Producer(yaml_config, "maj_powercard_response") if "maj_powercard_response" in yaml_config['kafka']['producers'] else None
        self.data_powercard_response = Kafka_Producer(yaml_config, "data_powercard_response") if "data_powercard_response" in yaml_config['kafka']['producers'] else None
        self.maj_amex_response = Kafka_Producer(yaml_config, "maj_amex_response") if "maj_amex_response" in yaml_config['kafka']['producers'] else None
        self.maj_jcb_response = Kafka_Producer(yaml_config, "maj_jcb_response") if "maj_jcb_response" in yaml_config['kafka']['producers'] else None


class Kafka_Config:
    def __init__(self, yaml_config):
        self.consumers = Kafka_Consumers_Config(yaml_config) if "consumers" in yaml_config['kafka'] else None
        self.producers = Kafka_Producers_Config(yaml_config) if "producers" in yaml_config['kafka'] else None


class Kafka_Status(Enum):
    HANDLED = "HANDLED"
    UPDATED = "UPDATED"
    ERROR = "ERROR"
    IGNORED = "IGNORED"