import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './authservice';

export interface AdminUser {
  userId: number;
  nickname: string;
  email: string;
  puntuacionElo: number;
  rol: string;
}

export interface AdminUpdateUser {
  nickname: string;
  puntuacionElo: number;
  rol: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = `${environment.apiUrl}Admin`;

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getUsers(): Observable<AdminUser[]> {
    return this.http.get<AdminUser[]>(`${this.apiUrl}/users`, { headers: this.getAuthHeaders() });
  }

  updateUser(userId: number, userData: AdminUpdateUser): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${userId}`, userData, { headers: this.getAuthHeaders() });
  }

  deleteUser(userId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${userId}`, { headers: this.getAuthHeaders() });
  }
}