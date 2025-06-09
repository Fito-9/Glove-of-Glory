import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../services/authservice';
import { WebsocketService } from '../../services/websocket.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  matchFound: any = null;
  waitingMessage: string = '';
  authService = inject(AuthService);
  userElo = 1200; 
  

  constructor(private websocketService: WebsocketService) { }

  ngOnInit(): void {
    // Verificar si el WebSocket está conectado
    if (!this.websocketService.connected$.getValue()) {
      console.log('WebSocket no está conectado. Reconectando...');
      this.websocketService.connect();
    }

    // Suscribirse a los mensajes de matchmaking
    this.websocketService.matchmakingMessage$.subscribe(message => {
      if (message?.type === 'matchFound') {
        if (!this.matchFound) {
          this.matchFound = {};
        }
        this.matchFound.gameId = message.payload.gameId.toString();
        this.waitingMessage = '';
    
        const currentUserId = Number(localStorage.getItem('UserId'));
        console.log("Mi ID:", currentUserId);
        console.log("Player1 ID:", this.matchFound.player1Id);
        console.log("Player2 ID:", this.matchFound.player2Id);

        if (currentUserId === message.payload.player1Id) {
          localStorage.setItem('playerType', 'Player1');
          localStorage.setItem('playerName', 'Jugador'+currentUserId);;
        } else {
          localStorage.setItem('playerType', 'Player2');
          localStorage.setItem('playerName', 'Jugador'+currentUserId);
        }
    
        console.log(`Partida encontrada. Eres ${localStorage.getItem('playerType')}`);
      } else if (message?.type === 'waitingForMatch') {
        this.waitingMessage = message.payload;
      }
    });
    
  }

  buscarPartida(): void {
    // Verificar nuevamente si el WebSocket está conectado antes de empezar el matchmaking
    if (!this.websocketService.connected$.getValue()) {
      console.log('WebSocket no está conectado. Reconectando...');
      this.websocketService.connect();
    }

    // Empezar matchmaking
    this.websocketService.requestMatchmaking();
  }
}
