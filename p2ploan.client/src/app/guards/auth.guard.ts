import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { filter, map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): Observable<boolean> {
    // initialized$ false bo'lsa (checkSession hali tugamagan) kutadi.
    // true bo'lganda bir marta tekshiradi va tugatadi.
    return this.auth.initialized$.pipe(
      filter(initialized => initialized),
      take(1),
      map(() => {
        if (this.auth.isLoggedIn) return true;
        this.router.navigate(['/login']);
        return false;
      })
    );
  }
}
