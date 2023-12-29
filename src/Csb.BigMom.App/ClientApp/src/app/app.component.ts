import {Component, OnDestroy} from '@angular/core';
import {Breakpoints, BreakpointObserver} from "@angular/cdk/layout";
import {Subject} from "rxjs";
import {takeUntil} from "rxjs/operators";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnDestroy{
  public title:string = 'filiere-tpe-ui';
  destroyed = new Subject<void>();
  constructor(breakpointObserver: BreakpointObserver) {
    breakpointObserver
        .observe([
            Breakpoints.XSmall,
            Breakpoints.Small,
            Breakpoints.Medium,
            Breakpoints.Large,
            Breakpoints.XLarge,
        ])
        .pipe(takeUntil(this.destroyed));
  }
  
  ngOnDestroy(): void {
    this.destroyed.next();
    this.destroyed.complete();
  }
}
