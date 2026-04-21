import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoanSummary, LoanDetail, Repayment } from '../models';

@Injectable({ providedIn: 'root' })
export class LoanService {
  constructor(private http: HttpClient) {}

  getOpenLoans(page = 1, pageSize = 20) {
    return this.http.get<LoanSummary[]>(`/api/loans?page=${page}&pageSize=${pageSize}`);
  }

  getById(id: string) {
    return this.http.get<LoanDetail>(`/api/loans/${id}`);
  }

  getMyLoans() {
    return this.http.get<LoanSummary[]>('/api/loans/my');
  }

  create(dto: {
    amount: number;
    durationDays: number;
    interestRate: number;
    frequency: number;
    title?: string;
    description?: string;
  }) {
    return this.http.post<{ id: string; status: number; amount: number }>('/api/loans', dto);
  }

  accept(id: string) {
    return this.http.post<{ message: string }>(`/api/loans/${id}/accept`, {});
  }

  getRepayments(id: string) {
    return this.http.get<Repayment[]>(`/api/loans/${id}/repayments`);
  }
}
