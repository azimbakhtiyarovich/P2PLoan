import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { LoanService } from '../../services/loan.service';

@Component({
  selector: 'app-create-loan',
  standalone: false,
  templateUrl: './create-loan.component.html'
})
export class CreateLoanComponent {
  form = {
    title: '',
    description: '',
    amount: null as number | null,
    durationDays: null as number | null,
    interestRate: null as number | null,
    frequency: 2  // Monthly by default
  };

  error = '';
  success = '';
  loading = false;

  frequencies = [
    { value: 0, label: 'Kunlik' },
    { value: 1, label: 'Haftalik' },
    { value: 2, label: 'Oylik' },
    { value: 3, label: 'Yillik' }
  ];

  constructor(private loanService: LoanService, private router: Router) {}

  submit() {
    if (!this.form.amount || !this.form.durationDays || !this.form.interestRate) {
      this.error = 'Summa, muddat va foizni kiriting.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.loanService.create({
      amount: this.form.amount,
      durationDays: this.form.durationDays,
      interestRate: this.form.interestRate,
      frequency: this.form.frequency,
      title: this.form.title || undefined,
      description: this.form.description || undefined
    }).subscribe({
      next: res => {
        this.success = 'Loan muvaffaqiyatli yaratildi!';
        this.loading = false;
        setTimeout(() => this.router.navigate(['/my-loans']), 1500);
      },
      error: err => {
        this.error = err?.error?.message ?? err?.error ?? 'Loan yaratishda xato.';
        this.loading = false;
      }
    });
  }
}
