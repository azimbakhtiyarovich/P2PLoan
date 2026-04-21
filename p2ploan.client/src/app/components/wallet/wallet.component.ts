import { Component, OnInit } from '@angular/core';
import { WalletService } from '../../services/wallet.service';
import { PaymentService } from '../../services/payment.service';
import { WalletBalance, Transaction, transactionTypeLabel } from '../../models';

@Component({
  selector: 'app-wallet',
  standalone: false,
  templateUrl: './wallet.component.html'
})
export class WalletComponent implements OnInit {
  balance: WalletBalance | null = null;
  transactions: Transaction[] = [];
  loading = true;
  error = '';

  depositAmount: number | null = null;
  depositProvider = 4; // Wallet (simulated)
  depositMsg = '';
  depositError = '';
  depositing = false;

  transactionTypeLabel = transactionTypeLabel;

  providers = [
    { value: 0, label: 'Payme' },
    { value: 1, label: 'Click' },
    { value: 2, label: 'UzumPay' },
    { value: 3, label: 'Karta' },
    { value: 4, label: 'Hamyon' }
  ];

  constructor(
    private walletService: WalletService,
    private paymentService: PaymentService
  ) {}

  ngOnInit() {
    this.loadBalance();
    this.loadTransactions();
  }

  loadBalance() {
    this.walletService.getBalance().subscribe({
      next: b => (this.balance = b),
      error: () => {}
    });
  }

  loadTransactions() {
    this.loading = true;
    this.walletService.getTransactions(1, 30).subscribe({
      next: txs => {
        this.transactions = txs;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  deposit() {
    if (!this.depositAmount || this.depositAmount <= 0) {
      this.depositError = 'Miqdorni kiriting.';
      return;
    }
    this.depositing = true;
    this.depositMsg = '';
    this.depositError = '';
    this.paymentService.deposit(this.depositAmount, this.depositProvider).subscribe({
      next: res => {
        this.depositMsg = res.message || 'To\'lov yaratildi.';
        this.depositing = false;
        this.depositAmount = null;
        this.loadBalance();
        this.loadTransactions();
      },
      error: err => {
        this.depositError = err?.error?.message ?? err?.error ?? 'Depozit amalga oshmadi.';
        this.depositing = false;
      }
    });
  }
}
