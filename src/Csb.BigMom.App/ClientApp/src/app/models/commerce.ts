import {Mcc} from "./mcc";

export interface Commerce {
    id: number;
    identifiant: string;
    mcc: Mcc;
    nom: string;
    email: string;
    ef: string;
    date_affiliation: string;
    date_resiliation: string;
    hash: string;
}