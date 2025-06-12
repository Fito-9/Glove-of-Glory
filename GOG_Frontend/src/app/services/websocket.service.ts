import { inject, Injectable } from '@angular/core';
import { WebSocketSubject, webSocket } from 'rxjs/webSocket';
import { BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './authservice';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private socket$: WebSocketSubject<any> | null = null;
  public connected$ = new BehaviorSubject<boolean>(false);
  
  public matchmakingMessage$ = new Subject<any>();
  public matchState$ = new BehaviorSubject<any>(null);

  // --- PROPIEDADES AÑADIDAS PARA USUARIOS ONLINE ---
  private connectedUsers = new Set<number>();
  public onlineUsers$ = new BehaviorSubject<Set<number>>(new Set());
  // -------------------------------------------------

  private authService = inject(AuthService);
  private router = inject(Router);
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectInterval = 5000;

  constructor(private http: HttpClient) {}

  connect(): void {
    const token = this.authService.getToken(); 
    if (!token || this.socket$) return;

    const url = `${environment.socketUrl}?token=${token}`;
    this.socket$ = webSocket({
      url: url,
      serializer: msg => msg,
      deserializer: event => event.data,
      openObserver: { next: () => {
          console.log('WebSocket conectado');
          this.connected$.next(true);
          this.reconnectAttempts = 0;
      }},
      closeObserver: { next: () => {
          console.log('WebSocket desconectado');
          this.connected$.next(false);
          this.socket$ = null;
          // --- LIMPIAR LISTA DE USUARIOS AL DESCONECTAR ---
          this.connectedUsers.clear();
          this.onlineUsers$.next(new Set());
          // ---------------------------------------------
          this.reconnect();
      }}
    });

    this.socket$.subscribe({
      next: (message: any) => this.handleMessage(message),
      error: err => {
        console.error('Error en WebSocket:', err);
        this.socket$ = null;
        this.reconnect();
      }
    });
  }

  disconnect(): void {
    if (this.socket$) {
      this.socket$.complete();
      this.socket$ = null;
      this.connected$.next(false);
    }
  }

  private handleMessage(message: string): void {
    try {
      const parsed = JSON.parse(message);
      switch (parsed.Type) {
        // --- NUEVO CASE PARA MANEJAR LA LISTA DE USUARIOS ONLINE ---
        case 'onlineUsers':
          const userIds: number[] = parsed.Payload;
          this.connectedUsers = new Set(userIds);
          this.onlineUsers$.next(this.connectedUsers);
          break;
        // ---------------------------------------------------------
        case 'matchFound':
          this.matchmakingMessage$.next(parsed.Payload);
          this.router.navigate(['/match', parsed.Payload.roomId]);
          break;
        case 'waitingForMatch':
          this.matchmakingMessage$.next(parsed);
          break;
        case 'matchStateUpdate':
          this.matchState$.next(parsed.Payload);
          break;
        default:
          console.log('Mensaje desconocido:', parsed);
      }
    } catch (error) {
      console.error('Error al parsear mensaje:', error, 'Original:', message);
    }
  }
  
  // --- NUEVO MÉTODO PARA OBTENER LA LISTA ACTUAL ---
  getOnlineUsers(): Set<number> {
    return this.connectedUsers;
  }
  // ------------------------------------------------

  // ... (resto de métodos como requestMatchmaking, sendGameAction, etc. no cambian)
  
  inviteFriend(roomId: string, invitedUserId: number): void {
    this.sendGameAction('inviteFriend', roomId, { InvitedUserId: invitedUserId });
  }
  
  requestInitialRoomState(roomId: string): void {
    this.sendGameAction('requestInitialState', roomId, {});
  }

  requestMatchmaking(): void {
    if (this.socket$) {
      this.socket$.next(JSON.stringify({ Type: 'matchmakingRequest', Payload: {} }));
    }
  }

  private sendGameAction(type: string, roomId: string, payload: any): void {
    if (this.socket$) {
      const message = {
        Type: type,
        Payload: JSON.stringify({ RoomId: roomId, Payload: payload })
      };
      this.socket$.next(JSON.stringify(message));
    }
  }

  selectCharacter(roomId: string, characterName: string): void {
    this.sendGameAction('selectCharacter', roomId, { CharacterName: characterName });
  }

  banMaps(roomId: string, bannedMaps: string[]): void {
    this.sendGameAction('banMaps', roomId, { BannedMaps: bannedMaps });
  }

  pickMap(roomId: string, pickedMap: string): void {
    this.sendGameAction('pickMap', roomId, { PickedMap: pickedMap });
  }

  sendChatMessage(roomId: string, message: string): void {
    this.sendGameAction('sendChatMessage', roomId, { Message: message });
  }

  declareWinner(roomId: string, declaredWinnerId: number): void {
    this.sendGameAction('declareWinner', roomId, { DeclaredWinnerId: declaredWinnerId });
  }
  
  private reconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      setTimeout(() => this.connect(), this.reconnectInterval);
    } else {
      console.error('Máximo de intentos de reconexión alcanzado.');
    }
  }
}