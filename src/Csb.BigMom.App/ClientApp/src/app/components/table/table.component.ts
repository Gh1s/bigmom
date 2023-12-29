import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import {Search} from "../../models/search";
import {MatTableDataSource} from "@angular/material/table";
import {MatSort, Sort} from "@angular/material/sort";
import {MatPaginator} from "@angular/material/paginator";
import {TelecollecteService} from "../../services/telecollecte.service";
import {Observable} from "rxjs";
import {DateTimeService} from "../../services/date-time.service";
import {TableSortsService} from "../../services/table-sorts.service";
import {TableFiltersService} from "../../services/table-filters.service";
import {ElasticFilter} from "../../models/elasticFilter";
import {DateSelection} from "../../models/dateSelection";


@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.css']
})
export class TableComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  public tlcSource!: MatTableDataSource<Observable<Search>>;
  public ResultsLength: number = 0;
  public queryEvent: string = "";
  public statusEvent: string[] = [];
  public applicationEvent: string[] = [];
  public rangeDateEvent: any = {start: "", end: ""};
  public pageIndex: number = 0 ;
  public pageSizeOptions: number[] = [5, 15, 50, 100, 500] ;
  public pageSize: number = this.pageSizeOptions[1] ;
  public startTimeQuery: string = "00:00:00";
  public endTimeQuery: string = "23:59:59" ;

  /** Columns displayed in the table. Columns IDs can be added, removed, or reordered. */
  public displayedColumns: string[] = ["nomCommercant", "idCommercant", "mcc", "noSerieTPE", "numRang", "application",
    "identifiantMonetique", "idsa", "statutTLC", "horaireTLCPlanifie", "horodateTraitement"];
  public products!: any[];
  public sortedData!: any[];

  constructor(
      private telecollecteService: TelecollecteService,
      private dateTimeService: DateTimeService,
      private tableSortsService: TableSortsService,
      private tableFiltersService: TableFiltersService) {
  }
  
  public showTlcs(search: any) {
    this.telecollecteService.getData()
        .subscribe((data: any) => {
          this.products = data['results'];
          console.log(this.products);
          this.products.forEach((elem) => {
            elem.app.heure_tlc = this.dateTimeService.formatTelecollectionTime("1.", elem.app.heure_tlc);
          })
          this.tlcSource = new MatTableDataSource(this.products);
          this.tlcSource.paginator = this.paginator;
        })
  }
  
  public formatDateNullForFiltering(dateSelection: DateSelection) {
    if (((dateSelection === undefined) || dateSelection.start === null ) || (dateSelection.end === null) || 
        (dateSelection.start === "") || (dateSelection.end === "")) {
      return  dateSelection = {
        start: "",
        end: ""
      } ;
    } else {
      console.log(dateSelection.start + " " + dateSelection.end);
      return dateSelection = {
        start: dateSelection.start,
        end: dateSelection.end
      } ;
    }
  }
  
  public getGlobalChanges(event: ElasticFilter) {
    
    this.rangeDateEvent = this.formatDateNullForFiltering(event.dateRange) ;

    if (event.application === null || event.application === undefined) {
      this.applicationEvent.push("");
    } else {
      this.applicationEvent = [] ;
      this.applicationEvent = event.application;
    }

    if (event.query === null || event.query === undefined) {
      this.queryEvent = "";
    } else {
      this.queryEvent = event.query;
    }
    
    switch (true) {
      case event.status.ok && event.status.ko:
        this.statusEvent = [] ;
        this.statusEvent.push("");
        break ;
      case !event.status.ok && !event.status.ko:
        this.statusEvent = [] ;
        this.statusEvent.push("") ;
        break ;
      case event.status.ok && !event.status.ko:
        this.statusEvent = [] ;
        this.statusEvent.push("OK") ;
        break;
      case !event.status.ok && event.status.ko:
        this.statusEvent = [] ;
        this.statusEvent.push("KO") ;
        break;
      default:
        this.statusEvent = [] ;
        this.statusEvent.push("");
        break;
    }
    
    this.showTlcs(this.tableFiltersService.tableFilters(
        this.queryEvent, this.statusEvent, this.applicationEvent,
        this.dateTimeService.formatDateForElastic(this.rangeDateEvent.start, this.startTimeQuery),
        this.dateTimeService.formatDateForElastic(this.rangeDateEvent.end, this.endTimeQuery)))
    
  }
  
  public sortData(sort: Sort) {
    
    if (!sort.active || sort.direction === '') {
      this.sortedData = this.products;
      this.showTlcs(this.tableFiltersService.tableFilters(
          this.queryEvent, this.statusEvent, this.applicationEvent,
          this.dateTimeService.formatDateForElastic(this.rangeDateEvent.start, this.startTimeQuery),
          this.dateTimeService.formatDateForElastic(this.rangeDateEvent.end, this.endTimeQuery)));
    }

    let dateSelection: any;
    
    switch (sort.active) {
      
      // @ts-ignore
      case 'nomCommercant':
        
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsCommerce("asc",
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery),
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsCommerce("desc", 
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }
        // @ts-ignore  
      case 'idCommercant':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsCommerceId("asc", 
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsCommerceId("desc",  
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }

        // @ts-ignore  
      case 'mcc':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            console.log(sort.direction) ;
            return this.showTlcs(this.tableSortsService.tableSortsMcc("asc",
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            console.log(sort.direction) ;
            return this.showTlcs(this.tableSortsService.tableSortsMcc("desc",
                 this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }
        // @ts-ignore  
      case 'noSerieTPE':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsNoSerie(
                "asc" , this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsNoSerie(
                "desc" , this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }
        // @ts-ignore  
      case 'numRang':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsNoSite("asc",  
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsNoSite( "desc", 
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }

        // @ts-ignore  
      case 'application':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsContrat("asc",  
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsContrat( "desc",  
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }
        // @ts-ignore  
      case 'identifiantMonetique':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsMonetiqueId("asc",  
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsMonetiqueId("desc", 
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }
        // @ts-ignore  
      case 'idsa':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsIdsa("asc", 
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsIdsa("desc",  
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }

        // @ts-ignore  
      case 'statutTLC':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsStatus( "asc",
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsStatus( "desc",
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }

      //need need to enhance the api because we can't today sort by telecollection timetable  
      /*case 'horaireTLCPlanifie':
        switch (sort.direction) {
          case "asc":
            return this.showTlcs(this.tableSortsService.tableSortHeureTlc("asc"));
          case "desc":
            return this.showTlcs(this.tableSortsService.tableSortHeureTlc("desc"));
        }*/
            
        // @ts-ignore  
      case 'horodateTraitement':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsProcessingDate(
                "asc", this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsProcessingDate(
                "desc", this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }

        // @ts-ignore  
      case 'montantCompenseTransaction':
        switch (sort.direction) {
          case "asc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent) ;
            return this.showTlcs(this.tableSortsService.tableSortsTotalAmount( "asc", 
                this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
          case "desc":
            dateSelection = this.formatDateNullForFiltering(this.rangeDateEvent);
            return this.showTlcs(this.tableSortsService.tableSortsTotalAmount(
                "desc", this.queryEvent, this.statusEvent, this.applicationEvent,
                this.dateTimeService.formatDateForElastic(dateSelection.start, this.startTimeQuery), 
                this.dateTimeService.formatDateForElastic(dateSelection.end, this.endTimeQuery)));
        }

      default:
        // @ts-ignore
        this.showTlcs(this.tableFiltersService.tableFilters(
            this.queryEvent, this.statusEvent, this.applicationEvent,
            this.dateTimeService.formatDateForElastic(this.rangeDateEvent.start, this.startTimeQuery),
            this.dateTimeService.formatDateForElastic(this.rangeDateEvent.end, this.endTimeQuery))) ;
    }
    
  }
  
  ngOnInit(): void {
    this.showTlcs(this.tableFiltersService.tableFilters());
  }

  ngAfterViewInit(): void {
  }

}
