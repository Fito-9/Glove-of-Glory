import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/authservice';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { WebsocketService } from '../../services/websocket.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
// La barra de navegación de arriba.
export class HeaderComponent implements OnInit, OnDestroy {
  authService = inject(AuthService);
  router = inject(Router);
  websocketService = inject(WebsocketService);

  private inviteSubscription!: Subscription;

  ngOnInit(): void {
    // Se queda escuchando por si llega una invitación a partida.
    this.inviteSubscription = this.websocketService.gameInvite$.subscribe(invite => {
      const accepted = confirm(`${invite.inviterName} te ha invitado a una partida. ¿Aceptar?`);
      if (accepted) {
        this.websocketService.acceptInvite(invite.inviterId);
      }
    });
  }

  ngOnDestroy(): void {
    this.inviteSubscription?.unsubscribe();
  }

  logout(): void {
    this.authService.logout();
  }
}