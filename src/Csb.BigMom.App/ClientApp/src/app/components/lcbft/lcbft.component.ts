import {AfterViewInit, Component, OnInit, ViewChild} from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import {MatTable, MatTableDataSource} from '@angular/material/table';
import { LcbftDataSource, LcbftItem } from './lcbft-datasource';
import {Observable} from "rxjs";
import {Search} from "../../models/search";
import {LcbftService} from "../../services/lcbft.service";

@Component({
  selector: 'app-lcbft',
  templateUrl: './lcbft.component.html',
  styleUrls: ['./lcbft.component.css']
})
export class LcbftComponent implements AfterViewInit, OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatTable) table!: MatTable<LcbftItem>;
  dataSource: LcbftDataSource;
  public elasticSource!: MatTableDataSource<Observable<any>>;
  public data!: any[];

  /** Columns displayed in the table. Columns IDs can be added, removed, or reordered. */
  displayedColumns = ['id', 'name'];

  constructor(private lcbftService: LcbftService) {
    this.dataSource = new LcbftDataSource();
  }
  
  public getDataFromElastic(){
    this.lcbftService.getData().subscribe(data => {
      console.log(data);
      console.log(this.data);
      this.elasticSource = new MatTableDataSource<Observable<any[]>>(this.data);
      console.log(this.elasticSource);
    })
  }

  ngOnInit(): void {
    this.getDataFromElastic();
  }
  
  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.table.dataSource = this.dataSource;
  }
}
