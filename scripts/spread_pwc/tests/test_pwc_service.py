import unittest
from scripts.spread_pwc.services.pwc_service import *


class MyTestCase(unittest.TestCase):

    def test_mise_a_jour_tpe_bancaire(self):
        messages = [
            {
                "trace_identifier": "000003",
                "commerce_id": "8910442",
                "no_serie_tpe": "",
                "no_contrat": "8910442",
                "no_site_tpe": "001",
                "application_code": "JADE",
                "heure_tlc": "2021-06-09T14:18:00",
                "ef": "014889"
            },
            {
                "trace_identifier": "000006",
                "commerce_id": "3260001",
                "no_serie_tpe": "84616252",
                "no_contrat": "3260001",
                "no_site_tpe": "001",
                "application_code": "EMV",
                "heure_tlc": "2021-06-09T23:13:00",
                "ef": "014889"
            },
            {
                "trace_identifier": "000009",
                "commerce_id": "8960001",
                "no_serie_tpe": "23944088",
                "no_contrat": "8960001",
                "no_site_tpe": "001",
                "application_code": "NFC",
                "heure_tlc": "2021-06-09T09:19:00",
                "ef": "014889"
            }
        ]

        for msg in messages:
            if msg["application_code"] == "EMV" or msg["application_code"] == "NFC" or msg["application_code"] == "PNF":
                msg["heure_tlc"] = transformation_dates_emv(msg["heure_tlc"])
                update_applis_bancaire_pwc(msg["commerce_id"], msg["no_serie_tpe"], msg["no_site_tpe"],
                                           msg["heure_tlc"], msg["application_code"], msg["ef"])
            elif msg["application_code"] == "JADE":
                msg["heure_tlc"] = transformation_dates_priv(msg["heure_tlc"])
                update_applis_privative_pwc(
                    msg["ef"], msg["no_site_tpe"], msg["commerce_id"], msg["heure_tlc"])


if __name__ == '__main__':
    unittest.main()
