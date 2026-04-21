import { Component, OnInit } from '@angular/core';
import { InvestmentService } from '../../services/investment.service';
import { Investment } from '../../models';

@Component({
  selector: 'app-investments',
  standalone: false,
  templateUrl: './investments.component.html'
})
export class InvestmentsComponent implements OnInit {
  investments: Investment[] = [];
  loading = true;
  error = '';
  withdrawMsg = '';
  withdrawError = '';

  constructor(private investService: InvestmentService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.investService.getMyInvestments().subscribe({
      next: data => {
        this.investments = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Investitsiyalarni yuklashda xato.';
        this.loading = false;
      }
    });
  }

  withdraw(id: string) {
    this.withdrawMsg = '';
    this.withdrawError = '';
    this.investService.withdraw(id).subscribe({
      next: res => {
        this.withdrawMsg = res.message || 'Investitsiya qaytarildi.';
        this.load();
      },
      error: err => {
        this.withdrawError = err?.error?.message ?? err?.error ?? 'Qaytarib bo\'lmadi.';
      }
    });
  }
}
