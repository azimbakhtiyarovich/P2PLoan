import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WalletBalance, Transaction } from '../models';

@Injectable({ providedIn: 'root' })
export class WalletService {
  constructor(private http: HttpClient) {}

  getBalance() {
    return this.http.get<WalletBalance>('/api/wallet/balance');
  }

  getTransactions(page = 1, pageSize = 20) {
    return this.http.get<Transaction[]>(
      `/api/wallet/transactions?page=${page}&pageSize=${pageSize}`
    );
  }
}
