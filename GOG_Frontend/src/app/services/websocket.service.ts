import { inject, Injectable, effect } from '@angular/core';
import { WebSocketSubject, webSocket } from 'rxjs/webSocket';
import { BehaviorSubject, Subject, EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { AuthService } from './authservice';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
// Este servicio maneja toda la magia de la comunicación en tiempo real.
export class WebsocketService {
  private socket$: WebSocketSubject<any> | null = null;
  public connected$ = new BehaviorSubject<boolean>(false);
  
  // Canales para notificar a los componentes sobre eventos específicos.
  public matchmakingMessage$ = new Subject<any>();
  public matchState$ = new BehaviorSubject<any>(null);
  public voteMismatch$ = new Subject<void>();
  public gameInvite$ = new Subject<{ inviterId: number, inviterName: string }>();
  public onlineUsers$ = new BehaviorSubject<Set<number>>(new Set());

  private connectedUsers = new Set<number>();

  private authService = inject(AuthService);
  private router = inject(Router);
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectInterval = 5000;

  constructor() {
    // Esto es un "effect" de Angular Signals.
    // Se ejecuta automáticamente cuando el estado del usuario cambia (login/logout).
    effect(() => {
      const user = this.authService.currentUserSig();
      if (user) {
        // Si hay un usuario y no estamos conectados, nos conectamos.
        if (!this.connected$.getValue()) {
          this.connect();
        }
      } else {
        // Si no hay usuario (logout), nos desconectamos.
        this.disconnect();
      }
    });
  }

  // Inicia la conexión con el servidor WebSocket.
  connect(): void {
    const token = this.authService.getToken();
    if (!token) {
      console.error("No hay token, no se puede conectar al WebSocket.");
      return;
    }
    if (this.socket$ && !this.socket$.closed) {
      return; // Ya estamos conectados.
    }

    const url = `${environment.socketUrl}?token=${token}`;
    this.socket$ = webSocket({
      url: url,
      serializer: msg => JSON.stringify(msg),
      deserializer: event => JSON.parse(event.data),
      openObserver: {
        next: () => {
          console.log('WebSocket conectado.');
          this.connected$.next(true);
          this.reconnectAttempts = 0;
        }
      },
      closeObserver: {
        next: () => {
          console.log('WebSocket desconectado.');
          this.connected$.next(false);
          this.socket$ = null;
          this.onlineUsers$.next(new Set());
          // Si el usuario sigue logueado, intentamos reconectar.
          if (this.authService.isLoggedIn()) {
            this.reconnect();
          }
        }
      }
    });

    // Nos suscribimos para empezar a recibir mensajes.
    this.socket$.pipe(
      catchError(err => {
        console.error('Error en WebSocket:', err);
        this.socket$?.complete();
        this.socket$ = null;
        if (this.authService.isLoggedIn()) {
          this.reconnect();
        }
        return EMPTY;
      })
    ).subscribe({
      next: (message: any) => this.handleMessage(message)
    });
  }

  // Cierra la conexión de forma limpia.
  disconnect(): void {
    if (this.socket$) {
      this.socket$.complete();
      this.socket$ = null;
      this.connected$.next(false);
      console.log('WebSocket desconectado por logout.');
    }
  }

  // El "cerebro" que decide qué hacer con cada mensaje que llega del servidor.
  private handleMessage(parsed: any): void {
    console.log("Mensaje recibido:", parsed);
    switch (parsed.Type) {
      case 'onlineUsers':
        this.onlineUsers$.next(new Set(parsed.Payload));
        break;
      case 'matchFound':
        this.router.navigate(['/match', parsed.Payload.roomId]);
        break;
      case 'waitingForMatch':
        this.matchmakingMessage$.next(parsed);
        break;
      case 'matchStateUpdate':
        this.matchState$.next(parsed.Payload);
        break;
      case 'voteMismatch':
        this.voteMismatch$.next();
        break;
      case 'gameInviteReceived':
        this.gameInvite$.next(parsed.Payload);
        break;
      case 'eloUpdate':
        if (parsed.Payload?.newElo) {
          this.authService.updateUserElo(parsed.Payload.newElo);
        }
        break;
      default:
        console.log('Mensaje de tipo desconocido:', parsed);
    }
  }

  getOnlineUsers(): Set<number> {
    return this.onlineUsers$.getValue();
  }

  // Método base para enviar cualquier mensaje al servidor.
  private send(message: any): void {
    if (this.socket$ && this.connected$.getValue()) {
      this.socket$.next(message);
    } else {
      console.error('Intento de enviar mensaje pero el WebSocket no está conectado.');
    }
  }

  // --- Métodos públicos para que los componentes envíen acciones ---

  requestMatchmaking(): void {
    this.send({ Type: 'matchmakingRequest', Payload: {} });
  }

  inviteFriend(friendId: number): void {
    this.send({ Type: 'inviteFriend', Payload: { InvitedUserId: friendId } });
  }

  acceptInvite(inviterId: number): void {
    this.send({ Type: 'acceptInvite', Payload: { InvitedUserId: inviterId } });
  }

  // Método genérico para acciones de juego.
  private sendGameAction(type: string, payload: any): void {
    this.send({ Type: type, Payload: payload });
  }

  requestInitialRoomState(roomId: string): void {
    this.sendGameAction('requestInitialState', { RoomId: roomId });
  }

  selectCharacter(roomId: string, characterName: string): void {
    this.sendGameAction('selectCharacter', { RoomId: roomId, CharacterName: characterName });
  }

  banMaps(roomId: string, bannedMaps: string[]): void {
    this.sendGameAction('banMaps', { RoomId: roomId, BannedMaps: bannedMaps });
  }

  pickMap(roomId: string, pickedMap: string): void {
    this.sendGameAction('pickMap', { RoomId: roomId, PickedMap: pickedMap });
  }

  sendChatMessage(roomId: string, message: string): void {
    this.sendGameAction('sendChatMessage', { RoomId: roomId, Message: message });
  }

  declareWinner(roomId: string, declaredWinnerId: number): void {
    this.sendGameAction('declareWinner', { RoomId: roomId, DeclaredWinnerId: declaredWinnerId });
  }
  
  // Intenta reconectar si se cae la conexión.
  private reconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      console.log(`Intento de reconexión ${this.reconnectAttempts}/${this.maxReconnectAttempts}...`);
      setTimeout(() => this.connect(), this.reconnectInterval);
    } else {
      console.error('No se pudo reconectar. Por favor, recarga la página.');
    }
  }
}