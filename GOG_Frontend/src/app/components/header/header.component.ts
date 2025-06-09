import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/authservice';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent {
  authService = inject(AuthService);
  router = inject(Router);

  logout(): void {
    this.authService.logout();
  }
}
