import {Injectable} from '@angular/core';
import * as moment from 'moment';

@Injectable({
  providedIn: 'root'
})
export class DateTimeService {

  constructor() { }
  
  public formatTelecollectionTime(removingExpression: string, processingDate: string) {
    return processingDate.replace(removingExpression, "") ;
  }

  public formatDateForElastic(dateString: string, timeString: string) {
    console.log(dateString)
    let localJsonDate: string = moment(dateString).format("YYYY-MM-DD" + "T" + timeString + "+11:00");
    console.log(localJsonDate) ;
    return localJsonDate
  }
  
}
