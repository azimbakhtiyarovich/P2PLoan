import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  constructor(private auth: AuthService) {}

  ngOnInit() {
    // Sahifa har yangilanganda cookie hali ham amal qilishini tekshiradi.
    // Muvaffaqiyatli bo'lsa UI sessiyani tiklaydi, aks holda login sahifaga yo'naltiradi.
    this.auth.checkSession().subscribe();
  }
}
