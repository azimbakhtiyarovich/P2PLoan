import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { WalletService } from '../../services/wallet.service';
import { LoanService } from '../../services/loan.service';
import { AuthResponse, LoanSummary, WalletBalance, loanStatusLabel } from '../../models';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  user: AuthResponse | null = null;
  balance: WalletBalance | null = null;
  loans: LoanSummary[] = [];
  loading = true;
  error = '';

  loanStatusLabel = loanStatusLabel;

  constructor(
    private auth: AuthService,
    private walletService: WalletService,
    private loanService: LoanService
  ) {}

  ngOnInit() {
    this.user = this.auth.currentUser$.value;
    this.walletService.getBalance().subscribe({
      next: b => (this.balance = b),
      error: () => {}
    });
    this.loanService.getOpenLoans(1, 5).subscribe({
      next: l => {
        this.loans = l;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  fundingPercent(loan: LoanSummary): number {
    if (!loan.amount) return 0;
    return Math.min(100, Math.round((loan.fundedAmount / loan.amount) * 100));
  }
}
