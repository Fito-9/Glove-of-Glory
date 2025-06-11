import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../../services/authservice';
import { FriendshipService, UserSummary, FriendRequest } from '../../services/friendship.service';
import { WebsocketService } from '../../services/websocket.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-friendship',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './friendship.component.html',
  styleUrl: './friendship.component.css'
})
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

    this.onlineUsersSubscription = this.websocketService.onlineUsers$.subscribe(onlineIds => {
        this.updateOnlineStatus(onlineIds);
    });
  }

  ngOnDestroy(): void {
    this.onlineUsersSubscription?.unsubscribe();
  }

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

  filterUsers(): void {
    if (!this.searchTerm) {
      this.filteredUsers = [];
      return;
    }
    this.filteredUsers = this.allUsers.filter(u => 
      u.nickname.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }

  updateOnlineStatus(onlineIds: Set<number>): void {
    this.friends.forEach(friend => friend.isOnline = onlineIds.has(friend.userId));
  }

  sendRequest(receiverId: number): void {
    if (!this.currentUserId) return;
    this.friendshipService.sendFriendRequest(this.currentUserId, receiverId).subscribe(() => {
      alert('Solicitud enviada.');
      // Opcional: actualizar estado para mostrar "Solicitud Enviada"
    });
  }

  acceptRequest(senderId: number): void {
    if (!this.currentUserId) return;
    this.friendshipService.acceptFriendRequest(senderId, this.currentUserId).subscribe(() => {
      alert('Amigo añadido.');
      this.loadAllData();
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
    // La roomId se genera en el backend, aquí podemos pasar un placeholder o nada.
    this.websocketService.inviteFriend("private-game", friendId);
  }

  isFriend(userId: number): boolean {
    return this.friends.some(f => f.userId === userId);
  }

  isRequestSent(userId: number): boolean {
    // Esta lógica es más compleja, por ahora la simplificamos.
    // En un sistema real, necesitarías saber las solicitudes que has enviado.
    return false;
  }
}