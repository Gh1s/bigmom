import unittest
from scripts.spread_jcb.services.jcb_service import *


class MyTestCase(unittest.TestCase):
    def test_jcb_services(self):
        messages = [
            {
                "trace_identifier": "000005",
                "commerce_id": "",
                "no_serie_tpe": "10956827",
                "no_contrat": "6901491",
                "no_site_tpe": "001",
                "application_code": "JCB",
                "heure_tlc": "2021-06-09T07:21:00",
                "ef": "018319"
            },
            {
                "trace_identifier": "000007",
                "commerce_id": "",
                "no_serie_tpe": "23944065",
                "no_contrat": "9900745",
                "no_site_tpe": "001",
                "application_code": "JCB",
                "heure_tlc": "2021-06-09T23:17:00",
                "ef": "018319"
            },
            {
                "trace_identifier": "000008",
                "commerce_id": "8910442",
                "no_serie_tpe": "",
                "no_contrat": "6908281",
                "no_site_tpe": "001",
                "application_code": "JCB",
                "heure_tlc": "2021-06-09T14:18:00",
                "ef": "014889"
            }
        ]
        for msg in messages:
            msg["heure_tlc"] = transformation_dates_jcb(msg["heure_tlc"])
            msg["adherent_number"] = get_adherent_number(msg['no_contrat'])
            merchant_list = append_jcb_list(
                msg["adherent_number"], msg["no_site_tpe"], msg["heure_tlc"])
            create_file_for_jcb(merchant_list)

    def tests_get_adherent_number(self):
        donnees_tests = [9960003,9960004,9960006,9960008,9960009,9960010,9960011,9960012,9960013,
                            9960014,9960017,9960018,9960019,9960021,9970004,9970005,9970008,9970009,
                            9970011,9970012,9970013,9970015,9980001,9980003,9980005,9980006,9980007,
                            9980008,9980048,9980049,9990002,9990005,6608951]

        liste_num_adherent = []
        for data in donnees_tests:
            num_adherent = get_adherent_number(data)
            liste_num_adherent.append(num_adherent)
        for elem in liste_num_adherent:
            print(elem)

if __name__ == '__main__':
    unittest.main()
