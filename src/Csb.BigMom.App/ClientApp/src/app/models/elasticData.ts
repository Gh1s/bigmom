import {Application} from "./application";
import {Terminal} from "./terminal";

export interface ElasticData {
    app: {
        id: number;
        heure_tlc: string;
        idsa: string;
        tpe: {
            id: number;
            no_serie: string;
            no_site: string;
            modele: string;
            statut: string;
            type_connexion: string;
            commerce: {
                id: number;
                identifiant: string;
                mcc: {
                    id: number;
                    code: string;
                    range_start: string;
                    range_end: string;
                }
                nom: string;
                email: string;
                ef: string;
                date_affiliation: string;
                date_resiliation: string;
                hash: string;
            }
        }
        contrat: {
            id: number;
            no_contrat: string;
            code: string;
            date_debut: string;
            date_fin: string;
            commerce: string;
        }
    };

}