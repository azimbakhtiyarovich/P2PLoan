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
  currentUser$ = new BehaviorSubject<UserSession | null>(null);

  private _initialized$ = new BehaviorSubject<boolean>(false);
  readonly initialized$ = this._initialized$.asObservable();

  constructor(private http: HttpClient) {
    this.checkSession();
  }

  get isLoggedIn(): boolean {
    return !!this.currentUser$.value;
  }

  register(email: string, phoneNumber: string, password: string, role: number = 0): Observable<UserSession> {
    return this.http
      .post<UserSession>('/api/auth/register', { email, phoneNumber, password, role })
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

  /** Sahifa yangilaganda cookie orqali sessiyani tiklash */
  private checkSession() {
    this.http.get<UserSession>('/api/auth/me').pipe(
      catchError(() => of(null))
    ).subscribe(session => {
      this.currentUser$.next(session);
      this._initialized$.next(true);
    });
  }
}
