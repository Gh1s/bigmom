import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TableSortsService {
  private skip: number = 0;
  private take: number = 1000

  constructor() { }
  
  public tableSortsCommerce(
      nomCommerceDirection: string = "", searchValue: string = "", 
      status: string[], application: string[] = [], startDate: string = "", endDate: string = "",
      skip: number = this.skip, take: number = this.take) {
    
    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.tpe.commerce.nom": nomCommerceDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsCommerceId(
      identifiantDirection: string = "", searchValue: string = "",
      status: string[], application: string[] = [], startDate: string = "", endDate: string = "",
      skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.tpe.commerce.identifiant": identifiantDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsMcc(
      mccDirection: string = "", searchValue: string = "",
      status: string[], application: string[] = [], startDate: string = "", endDate: string = "",
      skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.contrat.tpe.commerce.mcc.code": mccDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsNoSerie(
      noSerieDirection: string = "", searchValue: string = "",
      status: string[], application: string[] = [], startDate: string = "", endDate: string = "",
      skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.tpe.no_serie": noSerieDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsNoSite(
      noSiteDirection: string = "", searchValue: string = "", status: string[], application: string[] = [], 
      startDate: string = "", endDate: string = "", skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.tpe.no_site": noSiteDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsContrat(
      noContratDirection: string = "", searchValue: string = "", status: string[], application: string[] = [], 
      startDate: string = "", endDate: string = "", skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.contrat.code": noContratDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsMonetiqueId(
      noContratDirection: string = "", searchValue: string = "", status: string[], application: string[] = [],
      startDate: string = "", endDate: string = "", skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.contrat.no_contrat": noContratDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsIdsa(
      idsaDirection: string = "", searchValue: string = "", status: string[], application: string[] = [], 
      startDate: string = "", endDate: string = "", skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "app.idsa": idsaDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsStatus(
      statusDirection: string = "", searchValue: string = "",
      status: string[], application: string[] = [], startDate: string = "", endDate: string = "",
      skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "status": statusDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsProcessingDate(
      processingDateDirection: string = "", searchValue: string = "",
      status: string[], application: string[] = [], startDate: string = "", endDate: string = "",
      skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "processing_date": processingDateDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }

  public tableSortsTotalAmount(
      amountDirection: string = "", searchValue: string = "", status: string[], application: string[] = [], 
      startDate: string = "", endDate: string = "", skip: number = this.skip, take: number = this.take) {

    const sortQuery = {
      "search": searchValue,
      "filters": {
        "status": status,
        "app.contrat.code": application,
        "processing_date": {
          "start": startDate,
          "end": endDate
        }
      },
      "sorts": {
        "total_reconcilie": amountDirection
      },
      "skip": skip,
      "take": take
    }
    return sortQuery
  }
  
}
