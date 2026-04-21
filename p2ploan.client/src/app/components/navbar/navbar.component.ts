import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { AuthResponse } from '../../models';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.component.html'
})
export class NavbarComponent {
  user: AuthResponse | null = null;

  constructor(public auth: AuthService, private router: Router) {
    this.auth.currentUser$.subscribe(u => (this.user = u));
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
