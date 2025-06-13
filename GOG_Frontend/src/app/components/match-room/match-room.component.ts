import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef, AfterViewChecked, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { WebsocketService } from '../../services/websocket.service';
import { AuthService } from '../../services/authservice';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-match-room',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './match-room.component.html',
  styleUrls: ['./match-room.component.css']
})
export class MatchRoomComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('chatMessagesContainer') private chatContainer!: ElementRef;

  private route = inject(ActivatedRoute);
  private websocketService = inject(WebsocketService);
  public authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  roomId!: string;
  roomState: any;
  private stateSubscription!: Subscription;
  private voteMismatchSubscription!: Subscription;

  hasSelectedCharacter = false;
  selectedMapsForBan: string[] = [];
  chatMessage = '';
  
  voteMismatch = false;

  // ✅ NUEVA PROPIEDAD PARA CONTROLAR EL ESTADO DEL VOTO
  iHaveVoted = false;

  ngOnInit(): void {
    this.roomId = this.route.snapshot.paramMap.get('roomId')!;
    
    this.stateSubscription = this.websocketService.matchState$.subscribe(state => {
      if (state && state.roomId === this.roomId) {
        this.roomState = state;

        // ✅ INICIO DE LA LÓGICA MODIFICADA
        // Calculamos el estado del voto aquí y lo guardamos en la propiedad
        const myId = this.authService.currentUserSig()?.usuarioId;
        if (myId) {
          if (myId === this.roomState.player1Id) {
            this.iHaveVoted = this.roomState.player1Voted;
          } else if (myId === this.roomState.player2Id) {
            this.iHaveVoted = this.roomState.player2Voted;
          } else {
            this.iHaveVoted = true; // Es un espectador, deshabilitar siempre
          }
        } else {
          this.iHaveVoted = true; // No hay usuario, deshabilitar
        }
        // ✅ FIN DE LA LÓGICA MODIFICADA

        this.cdr.detectChanges();
      }
    });

    this.voteMismatchSubscription = this.websocketService.voteMismatch$.subscribe(() => {
      this.voteMismatch = true;
      setTimeout(() => this.voteMismatch = false, 3000);
    });

    this.websocketService.requestInitialRoomState(this.roomId);
  }

  ngOnDestroy(): void {
    this.stateSubscription?.unsubscribe();
    this.voteMismatchSubscription?.unsubscribe();
    this.websocketService.matchState$.next(null);
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      if (this.chatContainer) {
        this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
      }
    } catch (err) { }
  }

  onSelectCharacter(character: string): void {
    if (!this.hasSelectedCharacter) {
      this.websocketService.selectCharacter(this.roomId, character);
      this.hasSelectedCharacter = true;
    }
  }

  onMapClick(map: string): void {
    if (!this.isMyTurnToBanOrPick()) return;

    if (this.roomState.currentState.includes('Ban')) {
      const requiredBans = this.roomState.currentState === 'MapBanP1' ? 3 : 4;
      if (this.selectedMapsForBan.includes(map)) {
        this.selectedMapsForBan = this.selectedMapsForBan.filter(m => m !== map);
      } else if (this.selectedMapsForBan.length < requiredBans) {
        this.selectedMapsForBan.push(map);
      }
    } else if (this.roomState.currentState === 'MapPickP1') {
      this.websocketService.pickMap(this.roomId, map);
    }
  }

  confirmBans(): void {
    this.websocketService.banMaps(this.roomId, this.selectedMapsForBan);
    this.selectedMapsForBan = [];
  }

  onSendMessage(): void {
    if (this.chatMessage.trim()) {
      this.websocketService.sendChatMessage(this.roomId, this.chatMessage);
      this.chatMessage = '';
    }
  }

  onDeclareWinner(winnerId: number): void {
    this.websocketService.declareWinner(this.roomId, winnerId); 
  }

  shouldShowMatchup(): boolean {
    if (!this.roomState) return false;
    return this.roomState.currentState !== 'CharacterSelection' && this.roomState.currentState !== 'WaitingForPlayers';
  }
  
  isMyTurnToBanOrPick(): boolean {
    if (!this.roomState || !this.authService.currentUserSig()) return false;
    const myId = this.authService.currentUserSig()!.usuarioId;
    const { currentState, player1Id } = this.roomState;
    return (currentState === 'MapBanP1' && myId === player1Id) ||
           (currentState === 'MapBanP2' && myId !== player1Id) ||
           (currentState === 'MapPickP1' && myId === player1Id);
  }

  isMapVetoState(): boolean {
    if (!this.roomState) return false;
    const state = this.roomState.currentState;
    return state === 'MapBanP1' || state === 'MapBanP2' || state === 'MapPickP1';
  }

  // ❌ LA FUNCIÓN hasVoted() HA SIDO ELIMINADA

  getOpponentId(): number {
    const myId = this.authService.currentUserSig()!.usuarioId;
    return myId === this.roomState.player1Id ? this.roomState.player2Id : this.roomState.player1Id;
  }

  getTitleForState(state: string): string {
    switch (state) {
      case 'CharacterSelection': return 'Selección de Personajes';
      case 'MapBanP1': return `Turno de Veto: ${this.roomState.player1Username}`;
      case 'MapBanP2': return `Turno de Veto: ${this.roomState.player2Username}`;
      case 'MapPickP1': return `Turno de Selección: ${this.roomState.player1Username}`;
      case 'WinnerDeclaration': return 'Declarar Ganador';
      case 'Finished': return 'Partida Terminada';
      default: return 'Sala de Partida';
    }
  }

  getMapVetoInstructions(): string {
    const myId = this.authService.currentUserSig()!.usuarioId;
    const isP1 = myId === this.roomState.player1Id;
    switch (this.roomState.currentState) {
      case 'MapBanP1': return isP1 ? 'Banea 3 mapas.' : `Esperando a que ${this.roomState.player1Username} banee 3 mapas.`;
      case 'MapBanP2': return !isP1 ? 'Banea 4 mapas de los restantes.' : `Esperando a que ${this.roomState.player2Username} banee 4 mapas.`;
      case 'MapPickP1': return isP1 ? 'Elige 1 mapa para jugar.' : `Esperando a que ${this.roomState.player1Username} elija el mapa.`;
      default: return '';
    }
  }

  getMapImageUrl(mapName: string): string {
    switch (mapName) {
      case "Battlefield":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img1.jpg';
      case "Final Destination":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img3.jpg';
      case "Smashville":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img44.jpg';
      case "Town and City":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img85_en.jpg';
      case "Pokémon Stadium 2":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img40.jpg';
      case "Kalos Pokémon League":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img79.jpg';
      case "Small Battlefield":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img104.jpg';
      case "Hollow Bastion":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_addition_img11.jpg';
      case "Yoshi's Story":
        return 'https://www.smashbros.com/assets_v2/img/stage/stage_img19.jpg';
      default:
        return '';
    }
  }
}