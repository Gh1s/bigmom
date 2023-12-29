import {ElasticFilter} from "./elasticFilter";
import {ElasticSorts} from "./elasticSorts";

export interface Search {
    "search": string,
    "filters": ElasticFilter,
    "sorts": ElasticSorts,
    "skip": number,
    "take": number
}