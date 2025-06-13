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
export class HomeComponent implements OnInit, OnDestroy {
  waitingMessage: string = '';
  authService = inject(AuthService);
  websocketService = inject(WebsocketService);
  router = inject(Router);

  // CORRECCIÓN: Obtenemos el ELO del servicio de autenticación
  get userElo(): number {
    return this.authService.currentUserSig()?.puntuacionElo ?? 1200;
  }
  
  private matchmakingSubscription!: Subscription;

  ngOnInit(): void {
    if (!this.websocketService.connected$.getValue()) {
      console.log('HomeComponent: WebSocket no conectado. Conectando...');
      this.websocketService.connect();
    }

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

  buscarPartida(): void {
    console.log("Botón 'Buscar Partida' presionado.");
    
    if (!this.websocketService.connected$.getValue()) {
      console.error('No se puede buscar partida, WebSocket no está conectado.');
      this.waitingMessage = 'Error de conexión. Intenta recargar la página.';
      return;
    }

    this.websocketService.requestMatchmaking();
    this.waitingMessage = 'Buscando oponente...';
  }
}