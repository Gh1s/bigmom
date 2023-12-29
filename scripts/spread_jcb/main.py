import json
from kafka import KafkaConsumer
from scripts.config.config import Kafka_Config, Kafka_Status
from scripts.spread_jcb.config.config import logger, yaml_config
from scripts.spread_jcb.services.jcb_service import *
from scripts.handles.producer_handles import handle_producer


def main():
    consumer = KafkaConsumer(kafka_consumer_jcb_config.topic,
                             bootstrap_servers=kafka_consumer_jcb_config.bootstrap_servers,
                             auto_offset_reset=kafka_consumer_jcb_config.auto_offset_reset,
                             enable_auto_commit=kafka_consumer_jcb_config.auto_commit,
                             auto_commit_interval_ms=kafka_consumer_jcb_config.auto_commit_interval_ms,
                             max_poll_interval_ms=kafka_consumer_jcb_config.max_poll_interval_ms,
                             group_id=kafka_consumer_jcb_config.group_id)

    for k_msg in consumer:

        j_msg = json.loads(k_msg.value)

        try:
            offset_info = {'offset': k_msg.offset}
            logger.info('Offset: %s | Kafka - incoming message : %s',
                        k_msg.offset,
                        k_msg.value.decode("utf-8").replace("\n", ""),
                        extra=offset_info)
            status = handle_messages_kafka(j_msg)
            logger.info('Offset: %s | status %s ',
                        str(k_msg.offset),
                        str(status),
                        extra=offset_info)

        except Exception as e:
            logger.error('Offset: %s | Erreur dans le traitement du message',
                         k_msg.offset,
                         exc_info=1,  # permet de logger la stack d'erreur
                         extra=offset_info)
            handle_producer(kafka_producer_jcb_config.topic, job_config.job_property,
                            Kafka_Status.ERROR.value, str(e), j_msg["trace_identifier"],
                            kafka_producer_jcb_config.bootstrap_servers, logger)
            logger.error('status %s ', str(Kafka_Status.ERROR.value))

        finally:
            if not kafka_consumer_jcb_config.auto_commit:
                consumer.commit()


if __name__ == "__main__":
    log_mode_debug()
    logger.info('######  Mise à jour des heures de télécollectes JCB - Started  ##### ')

    kafka_config = Kafka_Config(yaml_config)
    kafka_consumer_jcb_config = kafka_config.consumers.maj_jcb_request
    kafka_producer_jcb_config = kafka_config.producers.maj_jcb_response
    job_config = Job_Config(yaml_config)

    main()
