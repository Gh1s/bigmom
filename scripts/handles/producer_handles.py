import json
from kafka import KafkaProducer


def handle_producer(topic, job, status, error, identifier, broker, logger_config):
    s_msg = serialize_message(job, status, error, identifier)
    producer = KafkaProducer(value_serializer=lambda m: json.dumps(m, ensure_ascii=False).encode('utf-8'),
                             bootstrap_servers=broker)
    try:
        producer.send(topic, s_msg)
        producer.flush()

    except Exception as e:
        logger_config.error(e)


def serialize_message(job, status, error, identifier):
    msg = {"job": job, "status": status, "error": error, "trace_identifier": identifier}
    return msg
