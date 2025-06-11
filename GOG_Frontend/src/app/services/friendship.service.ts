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
  export class FriendshipService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}Friendship`;
    private userApiUrl = `${environment.apiUrl}User`;
  
    // Obtener todos los usuarios para el buscador
    getUsers(): Observable<UserSummary[]> {
      return this.http.get<UserSummary[]>(this.userApiUrl);
    }
  
    getFriends(userId: number): Observable<UserSummary[]> {
      return this.http.get<UserSummary[]>(`${this.apiUrl}/friends/${userId}`);
    }
  
    getPendingRequests(userId: number): Observable<FriendRequest[]> {
      return this.http.get<FriendRequest[]>(`${this.apiUrl}/pending-requests/${userId}`);
    }
  
    sendFriendRequest(senderId: number, receiverId: number): Observable<any> {
      return this.http.post(`${this.apiUrl}/send-request`, { senderId, receiverId });
    }
  
    acceptFriendRequest(senderId: number, receiverId: number): Observable<any> {
      return this.http.post(`${this.apiUrl}/accept-request`, { senderId, receiverId });
    }
  
    rejectFriendRequest(senderId: number, receiverId: number): Observable<any> {
      return this.http.post(`${this.apiUrl}/reject-request`, { senderId, receiverId });
    }
  }