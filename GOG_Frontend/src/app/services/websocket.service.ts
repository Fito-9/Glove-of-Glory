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
export class WebsocketService {
  private socket$: WebSocketSubject<any> | null = null;
  public connected$ = new BehaviorSubject<boolean>(false);
  
  public matchmakingMessage$ = new Subject<any>();
  public matchState$ = new BehaviorSubject<any>(null);
  public voteMismatch$ = new Subject<void>();

  private connectedUsers = new Set<number>();
  public onlineUsers$ = new BehaviorSubject<Set<number>>(new Set());

  private authService = inject(AuthService);
  private router = inject(Router);
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectInterval = 5000;

  constructor() {
    effect(() => {
      const user = this.authService.currentUserSig();
      if (user) {
        if (!this.connected$.getValue()) {
          this.connect();
        }
      } else {
        this.disconnect();
      }
    });
  }

  connect(): void {
    const token = this.authService.getToken();
    if (!token) {
      console.error("No hay token, no se puede conectar al WebSocket.");
      return;
    }
    if (this.socket$ && this.socket$.closed === false) {
      return;
    }

    const url = `${environment.socketUrl}?token=${token}`;
    this.socket$ = webSocket({
      url: url,
      serializer: msg => JSON.stringify(msg),
      deserializer: event => JSON.parse(event.data),
      openObserver: {
        next: () => {
          console.log('WebSocket conectado exitosamente.');
          this.connected$.next(true);
          this.reconnectAttempts = 0;
        }
      },
      closeObserver: {
        next: () => {
          console.log('WebSocket desconectado.');
          this.connected$.next(false);
          this.socket$ = null;
          this.connectedUsers.clear();
          this.onlineUsers$.next(new Set());
          if (this.authService.isLoggedIn()) {
            this.reconnect();
          }
        }
      }
    });

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

  disconnect(): void {
    if (this.socket$) {
      this.socket$.complete();
      this.socket$ = null;
      this.connected$.next(false);
      console.log('WebSocket desconectado por logout.');
    }
  }

  private handleMessage(parsed: any): void {
    console.log("Mensaje recibido del WebSocket:", parsed);
    switch (parsed.Type) {
      case 'onlineUsers':
        const userIds: number[] = parsed.Payload;
        this.connectedUsers = new Set(userIds);
        this.onlineUsers$.next(this.connectedUsers);
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
      default:
        console.log('Mensaje de tipo desconocido:', parsed);
    }
  }

  getOnlineUsers(): Set<number> {
    return this.connectedUsers;
  }

  private send(message: any): void {
    if (this.socket$ && this.connected$.getValue()) {
      this.socket$.next(message);
    } else {
      console.error('Intento de enviar mensaje pero el WebSocket no está conectado.');
    }
  }

  requestMatchmaking(): void {
    this.send({ Type: 'matchmakingRequest', Payload: {} });
  }

  private sendGameAction(type: string, roomId: string, payload: any): void {
    const message = {
      Type: type,
      Payload: { RoomId: roomId, Payload: payload }
    };
    this.send(message);
  }

  requestInitialRoomState(roomId: string): void {
    this.sendGameAction('requestInitialState', roomId, {});
  }
  
  inviteFriend(friendId: number): void {
    const message = {
        Type: 'inviteFriend',
        Payload: { InvitedUserId: friendId }
    };
    this.send(message);
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
      console.log(`Intento de reconexión ${this.reconnectAttempts}/${this.maxReconnectAttempts}...`);
      setTimeout(() => this.connect(), this.reconnectInterval);
    } else {
      console.error('Máximo de intentos de reconexión alcanzado. Por favor, recarga la página.');
    }
  }
}