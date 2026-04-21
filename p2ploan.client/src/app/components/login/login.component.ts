import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html'
})
export class LoginComponent {
  phoneNumber = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    if (!this.phoneNumber || !this.password) {
      this.error = 'Telefon raqam va parolni kiriting.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.login(this.phoneNumber, this.password).subscribe({
      next: () => this.router.navigate(['/']),
      error: err => {
        this.error = err?.error?.message ?? err?.error ?? 'Kirish muvaffaqiyatsiz.';
        this.loading = false;
      }
    });
  }
}
