import { Injectable } from '@angular/core';
import {Observable, of} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {MessageService} from "./message.service";
import {environment} from "../../environments/environment";
import { catchError } from 'rxjs/internal/operators/catchError';

@Injectable({
  providedIn: 'root'
})
export class TelecollecteService {
  private apiUrl: string = environment.elasticUrl ;
  //private apiUrl: string = "https://bigmom-recette.csb.nc/api/tlcs" ;

  httpOptionsPost = {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*'
    },
    
  }
  
  public searchData(search: string, skip: number, take: number)  {
    return {
      "search": search,
      "skip": skip,
      "take": take
    }
  };

  constructor(
    private http: HttpClient,
    private messageService: MessageService) {}

  public getData() {
    return this.http.post<any>(this.apiUrl, this.httpOptionsPost)
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

}
