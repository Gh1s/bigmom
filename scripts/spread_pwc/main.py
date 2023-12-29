import json
import threading
from kafka import KafkaConsumer

from scripts.spread_pwc.config.config import *
from scripts.config.config import Kafka_Status
from scripts.handles.producer_handles import handle_producer
from scripts.spread_pwc.services.pwc_service import *


def first_consumer():

    consumer = KafkaConsumer(kafka_consumer_pwc_data_config.topic,
                             bootstrap_servers=kafka_consumer_pwc_data_config.bootstrap_servers,
                             auto_offset_reset=kafka_consumer_pwc_data_config.auto_offset_reset,
                             enable_auto_commit=kafka_consumer_pwc_data_config.auto_commit,
                             auto_commit_interval_ms=kafka_consumer_pwc_data_config.auto_commit_interval_ms,
                             max_poll_interval_ms=kafka_consumer_pwc_data_config.max_poll_interval_ms,
                             group_id=kafka_consumer_pwc_data_config.group_id)

    for k_msg in consumer:
        try:
            j_msg = json.loads(k_msg.value)
            
            offset_info = {'offset': k_msg.offset}
            logger.info('Offset: %s | Kafka - incoming message : %s', k_msg.offset,
                        k_msg.value.decode("utf-8").replace("\n", ""), extra=offset_info)
            status = handle_data_request(j_msg)
            logger.info('Offset: %s | merchant idsa: %s ', str(
                k_msg.offset), str(status), extra=offset_info)

        except Exception as e:
            logger.error('Offset: %s | Erreur dans le traitement du message',
                         k_msg.offset,
                         exc_info=1,  # permet de logger la stack d'erreur
                         extra=offset_info)
            logger.error('status %s ', str(Kafka_Status.ERROR.value))

        finally:
            if not kafka_consumer_pwc_data_config.auto_commit:
                consumer.commit()


def second_consumer():

    consumer = KafkaConsumer(kafka_consumer_pwc_spreading_config.topic,
                             bootstrap_servers=kafka_consumer_pwc_spreading_config.bootstrap_servers,
                             auto_offset_reset=kafka_consumer_pwc_spreading_config.auto_offset_reset,
                             enable_auto_commit=kafka_consumer_pwc_spreading_config.auto_commit,
                             auto_commit_interval_ms=kafka_consumer_pwc_spreading_config.auto_commit_interval_ms,
                             max_poll_interval_ms=kafka_consumer_pwc_spreading_config.max_poll_interval_ms,
                             group_id=kafka_consumer_pwc_spreading_config.group_id)

    for k_msg in consumer:
        try:
            j_msg = json.loads(k_msg.value)

            offset_info = {'offset': k_msg.offset}
            logger.info('Offset: %s | Kafka - incoming message : %s', k_msg.offset,
                        k_msg.value.decode("utf-8").replace("\n", ""), extra=offset_info)
            status = handle_spread_message(j_msg)
            logger.info('Offset: %s | status %s ', str(
                k_msg.offset), str(status), extra=offset_info)

        except Exception as e:
            logger.error('Offset: %s | Erreur dans le traitement du message', k_msg.offset,
                         exc_info=1,  # permet de logger la stack d'erreur
                         extra=offset_info)
            handle_producer(kafka_producer_pwc_spreading_config.topic, job_config.job_pwc_property, Kafka_Status.ERROR.value,
                            str(e), j_msg["trace_identifier"], kafka_producer_pwc_spreading_config.bootstrap_servers, logger)
            logger.error('status %s ', str(Kafka_Status.ERROR.value))

        finally:
            if not kafka_consumer_pwc_spreading_config.auto_commit:
                consumer.commit()


def main():

    consumer_1 = threading.Thread(target=first_consumer)
    consumer_2 = threading.Thread(target=second_consumer)
    consumer_1.start()
    consumer_2.start()


if __name__ == "__main__":
    log_mode_debug()
    logger.info(
        '######  Mise à jour des heures de télécollectes - Started  ##### ')

    kafka_config = Kafka_Config(yaml_config)
    kafka_consumer_pwc_spreading_config = kafka_config.consumers.maj_powercard_request
    kafka_producer_pwc_spreading_config = kafka_config.producers.maj_powercard_response
    kafka_consumer_pwc_data_config = kafka_config.consumers.data_powercard_request
    kafka_producer_pwc_data_config = kafka_config.producers.data_powercard_response
    job_config = Job_Config(yaml_config)

    main()
