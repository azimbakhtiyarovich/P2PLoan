import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthGuard } from './guards/auth.guard';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoansListComponent } from './components/loans-list/loans-list.component';
import { LoanDetailComponent } from './components/loan-detail/loan-detail.component';
import { CreateLoanComponent } from './components/create-loan/create-loan.component';
import { MyLoansComponent } from './components/my-loans/my-loans.component';
import { WalletComponent } from './components/wallet/wallet.component';
import { InvestmentsComponent } from './components/investments/investments.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'loans', component: LoansListComponent, canActivate: [AuthGuard] },
  { path: 'loans/:id', component: LoanDetailComponent, canActivate: [AuthGuard] },
  { path: 'my-loans', component: MyLoansComponent, canActivate: [AuthGuard] },
  { path: 'create-loan', component: CreateLoanComponent, canActivate: [AuthGuard] },
  { path: 'wallet', component: WalletComponent, canActivate: [AuthGuard] },
  { path: 'investments', component: InvestmentsComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
