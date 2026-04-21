export enum LoanStatus {
  Created = 0,
  OpenForFunding = 10,
  PartiallyFunded = 20,
  Funded = 30,
  AcceptedByBorrower = 40,
  Active = 50,
  Repayment = 60,
  Paid = 70,
  Overdue = 80,
  Default = 90,
  Cancelled = 100
}

export enum RepaymentFrequency {
  Daily = 0,
  Weekly = 1,
  Monthly = 2,
  Yearly = 3
}

export enum PaymentProvider {
  Payme = 0,
  Click = 1,
  UzumPay = 2,
  Card = 3,
  Wallet = 4
}

export enum TransactionType {
  Deposit = 0,
  Investment = 1,
  RepaymentReceived = 2,
  ProfitCredit = 3,
  ProfitWithdrawal = 4,
  Fee = 5,
  Refund = 6
}

export interface AuthResponse {
  userId: string;
  accessToken: string;
  expiresAt: string;
  roles: string[];
  activeRole: string;
}

export interface LoanSummary {
  id: string;
  title: string;
  amount: number;
  fundedAmount: number;
  durationDays: number;
  interestRate: number;
  status: LoanStatus;
}

export interface Repayment {
  id: string;
  dueDate: string;
  amount: number;
  principalAmount: number;
  interestAmount: number;
  paidAmount: number;
  status: number;
}

export interface LoanDetail extends LoanSummary {
  description: string;
  createdAt: string;
  repayments: Repayment[];
}

export interface WalletBalance {
  userId: string;
  balance: number;
}

export interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  balanceAfter: number;
  createdAt: string;
}

export interface Investment {
  id: string;
  amount: number;
  investedAt: string;
  loanId?: string;
}

export function loanStatusLabel(status: LoanStatus): string {
  const map: Record<number, string> = {
    0: 'Yaratilgan',
    10: 'Moliyalashtirish uchun ochiq',
    20: 'Qisman moliyalashtirilgan',
    30: 'Moliyalashtirilgan',
    40: 'Qarz oluvchi tomonidan qabul qilingan',
    50: 'Faol',
    60: 'To\'lov jarayonida',
    70: 'To\'langan',
    80: 'Muddati o\'tgan',
    90: 'Default',
    100: 'Bekor qilingan'
  };
  return map[status] ?? String(status);
}

export function transactionTypeLabel(type: TransactionType): string {
  const map: Record<number, string> = {
    0: 'Depozit',
    1: 'Investitsiya',
    2: 'To\'lov qabul qilindi',
    3: 'Foyda',
    4: 'Foyda yechib olindi',
    5: 'Komissiya',
    6: 'Qaytarish'
  };
  return map[type] ?? String(type);
}
