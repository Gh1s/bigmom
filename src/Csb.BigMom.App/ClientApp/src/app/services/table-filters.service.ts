import { Injectable } from '@angular/core';
import {DateTimeService} from "./date-time.service";

@Injectable({
  providedIn: 'root'
})

export class TableFiltersService {
  private skip: number = 0;
  private take: number = 1000

  constructor() { }
  
  public tableFilters(
      freeQuery: string = "", status: string[] = [], application: string[] = [], startDate: string = "",
      endDate: string = "", processingDate: string = "desc", skip: number = this.skip, take: number = this.take) {
    const filterQuery = {
      "search": freeQuery,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "processing_date": processingDate
      },
      "skip": skip,
      "take": take
    }
    return filterQuery
  }

}
