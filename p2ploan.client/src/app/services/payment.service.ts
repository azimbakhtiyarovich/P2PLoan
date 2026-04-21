import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  constructor(private http: HttpClient) {}

  deposit(amount: number, provider: number) {
    return this.http.post<{ id: string; amount: number; status: number; message: string }>(
      '/api/payments/deposit',
      { amount, provider }
    );
  }

  getMyPayments() {
    return this.http.get<any[]>('/api/payments/my');
  }
}
