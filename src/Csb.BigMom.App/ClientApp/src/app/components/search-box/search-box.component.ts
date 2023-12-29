import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {Observable} from "rxjs";
import {FormBuilder, FormControl, FormGroup} from "@angular/forms";
import {DateTimeService} from "../../services/date-time.service";
import {ElasticFilter} from "../../models/elasticFilter";


export interface Status {
  ok: boolean;
  ko: boolean;
}

interface Application {
  value: string;
}

@Component({
  selector: 'app-search-box',
  templateUrl: './search-box.component.html',
  styleUrls: ['./search-box.component.css']
})
export class SearchBoxComponent implements OnInit {
  @Output() onGlobalChange = new EventEmitter<ElasticFilter>() ;

  public items!: Observable<any[]>;

  public range = new FormGroup({
    start: new FormControl(),
    end: new FormControl()
  });

  public searchValue: string = "";
  public status!: FormGroup ;
  public applications = new FormControl();
  public selection = new FormControl();

  public applicationList: Application[] = [
    {value: 'EMV'},
    {value: 'NFC'},
    {value: 'AMX'},
    {value: 'JCB'},
    {value: 'PNF'},
    {value: 'JADE'}
  ]

  constructor(
      fb: FormBuilder,
      private dateTimeService: DateTimeService) {
    this.status = fb.group({
      ok: false,
      ko: false
    })
  }

  ngOnInit() { }
  
  getAll(dateRangeSelection: any, applicationSelection: string[], querySelection: string, statusSelection: any) {
    let elasticFilter: ElasticFilter = {
      dateRange: dateRangeSelection,
      application: applicationSelection,
      query: querySelection,
      status: statusSelection
    }
    this.onGlobalChange.emit(elasticFilter) ;
  }
  
}
