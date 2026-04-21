import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  email = '';
  phoneNumber = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    if (!this.email || !this.phoneNumber || !this.password) {
      this.error = 'Barcha maydonlarni to\'ldiring.';
      return;
    }
    if (this.password.length < 8) {
      this.error = 'Parol kamida 8 ta belgidan iborat bo\'lishi kerak.';
      return;
    }
    this.loading = true;
    this.error = '';
    this.auth.register(this.email, this.phoneNumber, this.password).subscribe({
      next: () => this.router.navigate(['/']),
      error: err => {
        this.error = err?.error?.message ?? err?.error ?? 'Ro\'yxatdan o\'tish muvaffaqiyatsiz.';
        this.loading = false;
      }
    });
  }
}
