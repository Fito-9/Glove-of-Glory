import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/authservice';
import { FriendshipService, UserSummary, FriendRequest } from '../../services/friendship.service';
import { WebsocketService } from '../../services/websocket.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-friendship',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './friendship.component.html',
  styleUrls: ['./friendship.component.css']
})
// Componente para buscar amigos, ver solicitudes y la lista de amigos.
export class FriendshipComponent implements OnInit, OnDestroy {
  private friendshipService = inject(FriendshipService);
  private authService = inject(AuthService);
  private websocketService = inject(WebsocketService);

  allUsers: UserSummary[] = [];
  filteredUsers: UserSummary[] = [];
  friends: UserSummary[] = [];
  pendingRequests: FriendRequest[] = [];
  
  searchTerm = '';
  currentUserId = this.authService.currentUserSig()?.usuarioId;

  private onlineUsersSubscription!: Subscription;

  ngOnInit(): void {
    if (!this.currentUserId) return;
    this.loadAllData();

    // Nos suscribimos para saber qué amigos están conectados en tiempo real.
    this.onlineUsersSubscription = this.websocketService.onlineUsers$.subscribe((onlineIds: Set<number>) => {
        this.updateOnlineStatus(onlineIds);
    });
  }

  ngOnDestroy(): void {
    this.onlineUsersSubscription?.unsubscribe();
  }

  // Carga toda la información necesaria para esta página.
  loadAllData(): void {
    if (!this.currentUserId) return;
    
    this.friendshipService.getUsers().subscribe(users => {
      this.allUsers = users.filter(u => u.userId !== this.currentUserId);
      this.filterUsers();
    });

    this.friendshipService.getFriends(this.currentUserId).subscribe(friends => {
      this.friends = friends;
      this.updateOnlineStatus(this.websocketService.getOnlineUsers());
    });

    this.friendshipService.getPendingRequests(this.currentUserId).subscribe(requests => {
      this.pendingRequests = requests;
    });
  }

  // Filtra la lista de usuarios según lo que se escribe en el buscador.
  filterUsers(): void {
    if (!this.searchTerm) {
      this.filteredUsers = [];
      return;
    }
    this.filteredUsers = this.allUsers.filter(u => 
      u.nickname && u.nickname.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  // Pone el puntito verde a los amigos que están online.
  updateOnlineStatus(onlineIds: Set<number>): void {
    this.friends.forEach(friend => friend.isOnline = onlineIds.has(friend.userId));
  }

  sendRequest(receiverId: number): void {
    if (!this.currentUserId) return;
    this.friendshipService.sendFriendRequest(this.currentUserId, receiverId).subscribe(() => {
      alert('Solicitud enviada.');
      // Aquí podrías actualizar el estado para que ponga "Solicitud Enviada" sin recargar todo.
    });
  }

  acceptRequest(senderId: number): void {
    if (!this.currentUserId) return;
    this.friendshipService.acceptFriendRequest(senderId, this.currentUserId).subscribe(() => {
      alert('Amigo añadido.');
      this.loadAllData(); // Recargamos todo para que se actualicen las listas.
    });
  }

  rejectRequest(senderId: number): void {
    if (!this.currentUserId) return;
    this.friendshipService.rejectFriendRequest(senderId, this.currentUserId).subscribe(() => {
      alert('Solicitud rechazada.');
      this.loadAllData();
    });
  }

  inviteToGame(friendId: number): void {
    if (!this.currentUserId) return;
    this.websocketService.inviteFriend(friendId);
    alert(`Invitación enviada.`);
  }

  // Funciones de ayuda para la vista.
  isFriend(userId: number): boolean {
    return this.friends.some(f => f.userId === userId);
  }

  isRequestSent(userId: number): boolean {
    return false;
  }
}