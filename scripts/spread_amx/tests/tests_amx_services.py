import unittest
from scripts.spread_amx.services.amx_service import *


class MyTestCase(unittest.TestCase):

    def test_update_heure_telecoll_opcom(self):
        messages = [
            {
                "trace_identifier": "000010",
                "commerce_id": "3256970",
                "no_serie_tpe": "10956827",
                "no_contrat": "9195795760",
                "no_site_tpe": "001",
                "application_code": "AMEX",
                "heure_tlc": "2021-06-09T07:21:00",
                "ef": "018319"
            },
            {
                "trace_identifier": "000002",
                "commerce_id": "3256970",
                "no_serie_tpe": "23944065",
                "no_contrat": "9196871768",
                "no_site_tpe": "002",
                "application_code": "AMEX",
                "heure_tlc": "2021-06-09T23:17:00",
                "ef": "018319"
            }
        ]

        for msg in messages:
            date_datetime_amex = datetime.datetime.strptime(
                msg["heure_tlc"], '%Y-%m-%d' + 'T' + '%H:%M:%S')
            msg["heure_tlc"] = date_datetime_amex.strftime('%H%M')
            if len(msg["no_site_tpe"]) == 3 or len(msg["no_site_tpe"]) == 2:
                if len(msg["no_site_tpe"]) == 3:
                    msg["no_site_tpe"] = msg["no_site_tpe"][1:3]
                    print(msg["no_site_tpe"])

                elif len(msg["no_site_tpe"]) == 2:
                    msg["no_site_tpe"] = msg["no_site_tpe"]
                    print(msg["no_site_tpe"])
                send_tlc_hour_ace(msg["no_contrat"],
                                  msg["no_site_tpe"], msg["heure_tlc"])


    def test_send_tlc_hour_ace(self):
        donnees_tests = [{"no_contrat": "9194057725", "num_tpe": "03", "tlc_hour": "0501"},
                         {"no_contrat": "9194057725", "num_tpe": "04", "tlc_hour": "0502"},
                         {"no_contrat": "9196864052", "num_tpe": "01", "tlc_hour": "0503"},
                         {"no_contrat": "9194090502", "num_tpe": "01", "tlc_hour": "0504"},
                         {"no_contrat": "9196865802", "num_tpe": "01", "tlc_hour": "0505"},
                         {"no_contrat": "9194074506", "num_tpe": "03", "tlc_hour": "0506"},
                         {"no_contrat": "9194074506", "num_tpe": "05", "tlc_hour": "0507"},
                         {"no_contrat": "9194074506", "num_tpe": "02", "tlc_hour": "0508"},
                         {"no_contrat": "9193653920", "num_tpe": "01", "tlc_hour": "0509"},
                         {"no_contrat": "9194064069", "num_tpe": "01", "tlc_hour": "0501"},
                         {"no_contrat": "9196871768", "num_tpe": "01", "tlc_hour": "0501"},
                         {"no_contrat": "9196871768", "num_tpe": "02", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "01", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "02", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "03", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "04", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "05", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "06", "tlc_hour": "0501"},
                         {"no_contrat": "9195795760", "num_tpe": "07", "tlc_hour": "0501"},
                         {"no_contrat": "9194080206", "num_tpe": "01", "tlc_hour": "0501"},
                         {"no_contrat": "9196890586", "num_tpe": "01", "tlc_hour": "0501"},
                         {"no_contrat": "9194080743", "num_tpe": "01", "tlc_hour": "0501"},
                         {"no_contrat": "9871009254", "num_tpe": "06", "tlc_hour": "0501"},
                         {"no_contrat": "9871009254", "num_tpe": "07", "tlc_hour": "0501"},
                         {"no_contrat": "9194095360", "num_tpe": "02", "tlc_hour": "0501"}]

        for data in donnees_tests:
            send_tlc_hour_ace(data["no_contrat"], data["num_tpe"], data["tlc_hour"])

if __name__ == '__main__':
    unittest.main()
