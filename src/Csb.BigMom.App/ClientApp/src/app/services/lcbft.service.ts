import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {catchError} from "rxjs/internal/operators/catchError";
import {Observable, of} from "rxjs";
import {MessageService} from "./message.service";
//import {Client} from "@elastic/elasticsearch";

@Injectable({
  providedIn: 'root'
})
export class LcbftService {
  private elasticUrl = 'http://192.168.201.171:9200/transactions/_search';
  private static readonly NB_RESULTS_PAGE: number = 10000;
  private static readonly SCROLL_API_SEARCH_CONTEXT_DURATION: string = '1m';
  //private client!: Client;

  httpOptionsPost = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*'
    },
  }

  private body = {
    "size": 1000
  };
  
  constructor(
      private http: HttpClient,
      private messageService: MessageService) { }

  public getData() {
    return this.http.post<any[]>(this.elasticUrl, this.body, this.httpOptionsPost)
        .pipe(
            catchError(this.handleError('Error when getting data from ElasticSearch'))
        );
  }
  private log(message: string) {
    this.messageService.add(`TelecollectionService: ${message}`);
  }

  private handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(error); // log to console instead
      this.log(`${operation} failed: ${error.message}`);
      // Let the app keep running by returning an empty result.
      return of(result as T);
    };
  }
  
  /*private elasticsearchClientConnection(): Client{
    if (!this.client){
      this.client = new Client({
        node: 'http://localhost:9200'
      });
    }
    return this.client;
  }*/
  
  
  
}
