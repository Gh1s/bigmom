import {Mcc} from "./mcc";
import {Commerce} from "./commerce";

export interface Terminal {
    id: number;
    no_serie: string;
    no_site: string;
    modele: string;
    statut: string;
    type_connexion: string;
    commerce: Commerce;
}