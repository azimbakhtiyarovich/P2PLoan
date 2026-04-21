import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CredentialsInterceptor } from './interceptors/auth.interceptor';

import { NavbarComponent } from './components/navbar/navbar.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoansListComponent } from './components/loans-list/loans-list.component';
import { LoanDetailComponent } from './components/loan-detail/loan-detail.component';
import { CreateLoanComponent } from './components/create-loan/create-loan.component';
import { MyLoansComponent } from './components/my-loans/my-loans.component';
import { WalletComponent } from './components/wallet/wallet.component';
import { InvestmentsComponent } from './components/investments/investments.component';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    LoginComponent,
    RegisterComponent,
    DashboardComponent,
    LoansListComponent,
    LoanDetailComponent,
    CreateLoanComponent,
    MyLoansComponent,
    WalletComponent,
    InvestmentsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: CredentialsInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
