import datetime
import logging
import json
import psycopg2
import random

from kafka import KafkaProducer
from data_generator.config.config import Config

conf = Config()
logger = logging.getLogger("data_generator")


def get_apps(conn):
    with conn.cursor() as cursor:
        cursor.execute(
            "select id, heure_tlc from application where heure_tlc is not null")
        return map(lambda row: (row[0], row[1]), cursor.fetchall())


def generate_tlcs(conn, apps):
    logger.info("Generating data")
    with conn.cursor() as cursor:
        batch_count = 0
        request_base = "insert into tlc (app_id, processing_date, status, nb_trs_credit, \
                        total_credit, nb_trs_debit, total_debit, total_reconcilie) values "
        values = []
        for app in apps:
            days = random.randint(15, 30)
            for i in range(0, days):
                batch_count += 1
                values.insert(0, f"({app[0]},'{get_date(i, app[1])}','{get_random_status()}',{get_random_trs()})")
                if batch_count == 1000:
                    request = request_base + ",".join(values)
                    logger.debug(request)
                    cursor.execute(request)
                    batch_count = 0
                    values.clear()
    logger.info("Data generated")
    logger.info("Committing to database")
    conn.commit()
    logger.info("Committed to database")


def queue_index_requests(conn):
    producer = KafkaProducer(
        bootstrap_servers=[conf.kafka_config.producers.tlc_index_request.bootstrap_servers],
        value_serializer=lambda v: json.dumps(v).encode('utf-8')
    )
    with conn.cursor() as cursor:
        cursor.execute('select "Id" from tlc')
        for row in cursor.fetchall():
            message = {
                "tlc_id": row[0]
            }
            producer.send(topic=conf.kafka_config.producers.tlc_index_request.topic, value=message)
    producer.flush()
    producer.close()



def get_date(i, hour):
    date = datetime.datetime.now() + datetime.timedelta(days=0 - i)
    date = date.replace(hour=0, minute=0, second=0, microsecond=0)
    date = date + hour
    return date


def get_random_status():
    statuses = ["OK", "KO"]
    weights = [0.8, 0.2]
    return random.choices(statuses, weights)[0]


def get_random_trs():
    nb_trs_credit = random.randint(0, 5)
    nb_trs_debit = random.randint(0, 50)
    total_credit = 0
    total_debit = 0
    if nb_trs_credit > 0:
        total_credit = random.randint(1000, 100000)
    if nb_trs_debit > 0:
        total_debit = random.randint(50000, 1000000)
    return f"{nb_trs_credit},{total_credit},{nb_trs_debit},{total_debit},{total_debit - total_credit}"


if __name__ == '__main__':
    logger.info("Connecting to database")
    with psycopg2.connect(
        host=conf.db_config.host,
        port=conf.db_config.port,
        database=conf.db_config.database,
        user=conf.db_config.user,
        password=conf.db_config.password
    ) as conn:
        logger.info("Connected to database")
        apps = get_apps(conn)
        generate_tlcs(conn, apps)
        queue_index_requests(conn)
