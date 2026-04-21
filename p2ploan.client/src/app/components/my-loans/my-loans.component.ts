import { Component, OnInit } from '@angular/core';
import { LoanService } from '../../services/loan.service';
import { LoanSummary, LoanStatus, loanStatusLabel } from '../../models';

@Component({
  selector: 'app-my-loans',
  standalone: false,
  templateUrl: './my-loans.component.html'
})
export class MyLoansComponent implements OnInit {
  loans: LoanSummary[] = [];
  loading = true;
  error = '';
  actionMsg = '';
  actionError = '';

  LoanStatus = LoanStatus;
  loanStatusLabel = loanStatusLabel;

  constructor(private loanService: LoanService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.loanService.getMyLoans().subscribe({
      next: l => {
        this.loans = l;
        this.loading = false;
      },
      error: () => {
        this.error = 'Loanlarni yuklashda xato.';
        this.loading = false;
      }
    });
  }

  accept(id: string) {
    this.actionMsg = '';
    this.actionError = '';
    this.loanService.accept(id).subscribe({
      next: res => {
        this.actionMsg = res.message || 'Kredit qabul qilindi.';
        this.load();
      },
      error: err => {
        this.actionError = err?.error?.message ?? err?.error ?? 'Qabul qilishda xato.';
      }
    });
  }

  fundingPercent(loan: LoanSummary): number {
    if (!loan.amount) return 0;
    return Math.min(100, Math.round((loan.fundedAmount / loan.amount) * 100));
  }
}
