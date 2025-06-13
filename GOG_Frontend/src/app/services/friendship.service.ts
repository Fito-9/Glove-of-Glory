import { HttpClient } from "@angular/common/http";
import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../environments/environment";

export interface UserSummary {
  userId: number;
  nickname: string;
  ruta: string | null;
  isOnline?: boolean;
}
  
export interface FriendRequest {
  senderId: number;
  receiverId: number;
  senderNickname?: string;
  senderAvatar?: string;
}

@Injectable({
  providedIn: 'root'
})
// Servicio para todo lo que tenga que ver con amigos.
export class FriendshipService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}Friendship`;
  private userApiUrl = `${environment.apiUrl}User`;

  // Pide la lista de todos los usuarios para el buscador.
  getUsers(): Observable<UserSummary[]> {
    return this.http.get<UserSummary[]>(this.userApiUrl);
  }

  // Pide la lista de amigos de un usuario.
  getFriends(userId: number): Observable<UserSummary[]> {
    return this.http.get<UserSummary[]>(`${this.apiUrl}/friends/${userId}`);
  }

  // Pide las solicitudes de amistad pendientes.
  getPendingRequests(userId: number): Observable<FriendRequest[]> {
    return this.http.get<FriendRequest[]>(`${this.apiUrl}/pending-requests/${userId}`);
  }

  // Envía una solicitud de amistad.
  sendFriendRequest(senderId: number, receiverId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-request`, { senderId, receiverId });
  }

  // Acepta una solicitud.
  acceptFriendRequest(senderId: number, receiverId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/accept-request`, { senderId, receiverId });
  }

  // Rechaza una solicitud.
  rejectFriendRequest(senderId: number, receiverId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/reject-request`, { senderId, receiverId });
  }
}