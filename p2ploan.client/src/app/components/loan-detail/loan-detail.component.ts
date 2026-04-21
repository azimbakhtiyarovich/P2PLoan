import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LoanService } from '../../services/loan.service';
import { InvestmentService } from '../../services/investment.service';
import { LoanDetail, loanStatusLabel } from '../../models';

@Component({
  selector: 'app-loan-detail',
  standalone: false,
  templateUrl: './loan-detail.component.html'
})
export class LoanDetailComponent implements OnInit {
  loan: LoanDetail | null = null;
  loading = true;
  error = '';

  investAmount: number | null = null;
  investMsg = '';
  investError = '';
  investing = false;

  loanStatusLabel = loanStatusLabel;

  constructor(
    private route: ActivatedRoute,
    private loanService: LoanService,
    private investService: InvestmentService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loanService.getById(id).subscribe({
      next: l => {
        this.loan = l;
        this.loading = false;
      },
      error: () => {
        this.error = 'Loan topilmadi.';
        this.loading = false;
      }
    });
  }

  invest() {
    if (!this.loan || !this.investAmount || this.investAmount <= 0) {
      this.investError = 'Miqdorni to\'g\'ri kiriting.';
      return;
    }
    this.investing = true;
    this.investMsg = '';
    this.investError = '';
    this.investService.invest(this.loan.id, this.investAmount).subscribe({
      next: res => {
        this.investMsg = `Investitsiya muvaffaqiyatli: ${res.amount} UZS`;
        this.investing = false;
        this.investAmount = null;
        // Reload loan to update funded amount
        this.loanService.getById(this.loan!.id).subscribe(l => (this.loan = l));
      },
      error: err => {
        this.investError = err?.error?.message ?? err?.error ?? 'Investitsiya amalga oshmadi.';
        this.investing = false;
      }
    });
  }

  fundingPercent(): number {
    if (!this.loan?.amount) return 0;
    return Math.min(100, Math.round((this.loan.fundedAmount / this.loan.amount) * 100));
  }
}
