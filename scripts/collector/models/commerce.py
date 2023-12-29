
class Commerce:
    def __init__(self, identifiant, mcc, nom, ef, tpes, contrats, date_affiliation, date_resiliation):
        self.identifiant = identifiant
        self.mcc = mcc
        self.nom = nom
        self.ef = ef
        self.tpes = tpes
        self.contrats = contrats
        self.date_affiliation = date_affiliation
        self.date_resiliation = date_resiliation

    def to_dict(self):
        return {
            "identifiant": self.identifiant,
            "mcc": vars(self.mcc),
            "nom": self.nom,
            "ef": self.ef,
            "tpes": [vars(tpe) for tpe in self.tpes],
            "contrats": [vars(contrat) for contrat in self.contrats],
            "date_affiliation": self.date_affiliation,
            "date_resiliation":self.date_resiliation
        }
