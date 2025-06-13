import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../services/authservice';
import { WebsocketService } from '../../services/websocket.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
// La página principal donde buscas partida.
export class HomeComponent implements OnInit, OnDestroy {
  waitingMessage: string = '';
  authService = inject(AuthService);
  websocketService = inject(WebsocketService);
  router = inject(Router);

  get userElo(): number {
    return this.authService.currentUserSig()?.puntuacionElo ?? 1200;
  }
  
  private matchmakingSubscription!: Subscription;

  ngOnInit(): void {
    // Si por alguna razón el WebSocket no está conectado, lo conectamos.
    if (!this.websocketService.connected$.getValue()) {
      this.websocketService.connect();
    }

    // Escuchamos los mensajes de la cola de matchmaking.
    this.matchmakingSubscription = this.websocketService.matchmakingMessage$.subscribe(message => {
      if (message?.type === 'waitingForMatch') {
        this.waitingMessage = message.payload;
      } else {
        this.waitingMessage = '';
      }
    });
  }

  ngOnDestroy(): void {
    if (this.matchmakingSubscription) {
      this.matchmakingSubscription.unsubscribe();
    }
  }

  // Se llama al pulsar el botón de buscar partida.
  buscarPartida(): void {
    if (!this.websocketService.connected$.getValue()) {
      this.waitingMessage = 'Error de conexión. Intenta recargar la página.';
      return;
    }

    this.websocketService.requestMatchmaking();
    this.waitingMessage = 'Buscando oponente...';
  }
}