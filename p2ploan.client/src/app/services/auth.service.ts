import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { AuthResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'p2p_token';
  private readonly USER_KEY  = 'p2p_user';

  currentUser$ = new BehaviorSubject<AuthResponse | null>(this.loadUser());

  constructor(private http: HttpClient) {}

  get token(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  get isLoggedIn(): boolean {
    return !!this.token;
  }

  register(email: string, phoneNumber: string, password: string, role: number = 0) {
    return this.http
      .post<AuthResponse>('/api/auth/register', { email, phoneNumber, password, role })
      .pipe(tap(res => this.save(res)));
  }

  login(phoneNumber: string, password: string) {
    return this.http
      .post<AuthResponse>('/api/auth/login', { phoneNumber, password })
      .pipe(tap(res => this.save(res)));
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser$.next(null);
  }

  private save(res: AuthResponse) {
    localStorage.setItem(this.TOKEN_KEY, res.accessToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(res));
    this.currentUser$.next(res);
  }

  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem(this.USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }
}
