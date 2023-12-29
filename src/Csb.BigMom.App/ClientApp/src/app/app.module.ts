import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { TokenProvider } from './token-provider';
//import { AuthenticationInterceptor } from './authentication.interceptor';
//import { CorsInterceptor } from './cors.interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {MatIconModule} from "@angular/material/icon";
import {MatToolbarModule} from "@angular/material/toolbar";
import {MatTableModule} from "@angular/material/table";
import {MatDatepickerModule} from "@angular/material/datepicker";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatInputModule} from "@angular/material/input";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
//import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import {MatNativeDateModule, MatRippleModule} from "@angular/material/core";
import {MatButtonModule} from "@angular/material/button";
import {MatSortModule} from "@angular/material/sort";
import {MatPaginatorModule} from "@angular/material/paginator";
import {ScrollingModule} from "@angular/cdk/scrolling";
import {CdkTableModule} from "@angular/cdk/table";
import {MatSidenavModule} from "@angular/material/sidenav";
import {TableVirtualScrollModule} from "ng-table-virtual-scroll";
import {MatCheckboxModule} from "@angular/material/checkbox";
import {MatTooltipModule} from "@angular/material/tooltip";
import {MatSelectModule} from "@angular/material/select";
import {MatCardModule} from "@angular/material/card";
import {MatExpansionModule} from "@angular/material/expansion";
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import { LayoutModule } from '@angular/cdk/layout';
import { MatListModule } from '@angular/material/list';
import {MatGridListModule} from "@angular/material/grid-list";
import { SearchBoxComponent } from './components/search-box/search-box.component';
import { TableComponent } from './components/table/table.component';
import { NavigationComponent } from './components/navigation/navigation.component';
import {Routes} from "@angular/router";
import { LcbftComponent } from './components/lcbft/lcbft.component';
import {MatMenuModule} from "@angular/material/menu";

@NgModule({
  declarations: [
    AppComponent,
    SearchBoxComponent,
    TableComponent,
    NavigationComponent,
    LcbftComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatIconModule,
    MatToolbarModule,
    MatTableModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatButtonModule,
    FormsModule,
    MatSortModule,
    MatPaginatorModule,
    ScrollingModule,
    CdkTableModule,
    MatRippleModule,
    MatSidenavModule,
    TableVirtualScrollModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatSelectModule,
    MatCardModule,
    MatExpansionModule,
    HttpClientModule,
    LayoutModule,
    MatListModule,
    MatGridListModule,
    TableVirtualScrollModule,
    MatMenuModule,
  ],
  providers: [
    TokenProvider,
    //{ provide: HTTP_INTERCEPTORS, useClass: AuthenticationInterceptor, multi: true },
    //{ provide: HTTP_INTERCEPTORS, useClass: CorsInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
