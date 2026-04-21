import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of } from 'rxjs';

export interface UserSession {
  userId: string;
  expiresAt: string;
  roles: string[];
  activeRole: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Token SAQLANMAYDI. Faqat UI uchun foydalanuvchi ma'lumotlari.
  // Haqiqiy autentifikatsiya — HttpOnly cookie (backend boshqaradi).
  currentUser$ = new BehaviorSubject<UserSession | null>(null);

  // Guard bu signal bilan kutadi: checkSession tugaguncha route aktivlanmaydi
  private _initialized$ = new BehaviorSubject<boolean>(false);
  readonly initialized$ = this._initialized$.asObservable();

  constructor(private http: HttpClient) {}

  get isLoggedIn(): boolean {
    return !!this.currentUser$.value;
  }

  register(email: string, phoneNumber: string, password: string, role: number = 0) {
    return this.http
      .post<AuthResponse>('/api/auth/register', { email, phoneNumber, password, role })
      .pipe(tap(res => this.save(res)));
  }

  register(email: string, phoneNumber: string, password: string): Observable<UserSession> {
    return this.http
      .post<UserSession>('/api/auth/register', { email, phoneNumber, password })
      .pipe(tap(session => this.currentUser$.next(session)));
  }

  login(phoneNumber: string, password: string): Observable<UserSession> {
    return this.http
      .post<UserSession>('/api/auth/login', { phoneNumber, password })
      .pipe(tap(session => this.currentUser$.next(session)));
  }

  logout(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/auth/logout', {}).pipe(
      tap(() => this.currentUser$.next(null))
    );
  }
}
