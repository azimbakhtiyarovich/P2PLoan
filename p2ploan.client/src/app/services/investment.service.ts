import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Investment } from '../models';

@Injectable({ providedIn: 'root' })
export class InvestmentService {
  constructor(private http: HttpClient) {}

  getMyInvestments() {
    return this.http.get<Investment[]>('/api/investments/my');
  }

  invest(loanId: string, amount: number) {
    return this.http.post<{ id: string; amount: number; investedAt: string }>(
      '/api/investments',
      { loanId, amount }
    );
  }

  withdraw(investmentId: string) {
    return this.http.delete<{ message: string }>(`/api/investments/${investmentId}`);
  }
}
