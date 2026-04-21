import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, UserSession } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {
  user: UserSession | null = null;

  constructor(public auth: AuthService, private router: Router) {
    this.auth.currentUser$.subscribe(u => (this.user = u));
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login'])  // Hatto xato bo'lsa ham chiqarish
    });
  }
}
