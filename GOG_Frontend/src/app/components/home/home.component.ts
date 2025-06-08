import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/authservice';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  authService = inject(AuthService);
  // Simulación de ELO
  userElo = 1200; 
}
