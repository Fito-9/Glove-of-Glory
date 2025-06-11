import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
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

  roomId!: string;
  roomState: any;
  private stateSubscription!: Subscription;

  hasSelectedCharacter = false;
  selectedMapsForBan: string[] = [];
  chatMessage = '';

  ngOnInit(): void {
    this.roomId = this.route.snapshot.paramMap.get('roomId')!;
    this.stateSubscription = this.websocketService.matchState$.subscribe(state => {
      if (state && state.roomId === this.roomId) {
        this.roomState = state;
      }
    });
    // Solicitamos activamente el estado inicial por si nos lo perdimos.
    this.websocketService.requestInitialRoomState(this.roomId);
  }

  ngOnDestroy(): void {
    this.stateSubscription?.unsubscribe();
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

  // --- MÉTODOS AUXILIARES PARA LA VISTA ---

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

  hasVoted(): boolean {
    if (!this.roomState || !this.authService.currentUserSig()) return false;
    const myId = this.authService.currentUserSig()!.usuarioId;
    return (myId === this.roomState.player1Id && this.roomState.player1Voted) ||
           (myId !== this.roomState.player1Id && this.roomState.player2Voted);
  }

  getOpponentId(): number {
    const myId = this.authService.currentUserSig()!.usuarioId;
    return myId === this.roomState.player1Id ? this.roomState.player2Id : this.roomState.player1Id;
  }

  getTitleForState(state: string): string {
    switch (state) {
      case 'CharacterSelection': return 'Selección de Personajes';
      case 'MapBanP1': return 'Turno de Veto: Jugador 1';
      case 'MapBanP2': return 'Turno de Veto: Jugador 2';
      case 'MapPickP1': return 'Turno de Selección: Jugador 1';
      case 'WinnerDeclaration': return 'Declarar Ganador';
      case 'Finished': return 'Partida Terminada';
      default: return 'Sala de Partida';
    }
  }

  getMapVetoInstructions(): string {
    const myId = this.authService.currentUserSig()!.usuarioId;
    const isP1 = myId === this.roomState.player1Id;
    switch (this.roomState.currentState) {
      case 'MapBanP1': return isP1 ? 'Banea 3 mapas.' : 'Esperando a que el Jugador 1 banee 3 mapas.';
      case 'MapBanP2': return !isP1 ? 'Banea 4 mapas de los restantes.' : 'Esperando a que el Jugador 2 banee 4 mapas.';
      case 'MapPickP1': return isP1 ? 'Elige 1 mapa para jugar.' : 'Esperando a que el Jugador 1 elija el mapa.';
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
        return ''; // URL a una imagen por defecto
    }
  }
}