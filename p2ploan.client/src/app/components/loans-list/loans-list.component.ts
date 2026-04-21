import { Component, OnInit } from '@angular/core';
import { LoanService } from '../../services/loan.service';
import { LoanSummary, loanStatusLabel } from '../../models';

@Component({
  selector: 'app-loans-list',
  standalone: false,
  templateUrl: './loans-list.component.html'
})
export class LoansListComponent implements OnInit {
  loans: LoanSummary[] = [];
  loading = true;
  error = '';
  page = 1;

  loanStatusLabel = loanStatusLabel;

  constructor(private loanService: LoanService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.loanService.getOpenLoans(this.page, 20).subscribe({
      next: l => {
        this.loans = l;
        this.loading = false;
      },
      error: err => {
        this.error = 'Loanlarni yuklashda xato yuz berdi.';
        this.loading = false;
      }
    });
  }

  fundingPercent(loan: LoanSummary): number {
    if (!loan.amount) return 0;
    return Math.min(100, Math.round((loan.fundedAmount / loan.amount) * 100));
  }
}
