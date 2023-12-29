import json

from kafka import KafkaProducer

file = open("config\donnees_test.json")
file_json = json.load(file)
producer = KafkaProducer(bootstrap_servers='localhost:9092', value_serializer=lambda v: json.dumps(v).encode('utf-8'),
                         key_serializer=lambda m: json.dumps(m).encode('utf-8'))
for row in file_json:
    print(row.values())
    producer.send('', value=row)

