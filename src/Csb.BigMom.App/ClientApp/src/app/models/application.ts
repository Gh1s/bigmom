import {Terminal} from "./terminal";
import {Contrat} from "./contrat";

export interface Application {
    id: number;
    heureTlc: string;
    idsa: string;
    tpe: Terminal;
    contrat: Contrat;
}