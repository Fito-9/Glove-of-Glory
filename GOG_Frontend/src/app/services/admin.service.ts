import { HttpClient } from '@angular/common/http';
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
// Servicio para las acciones del panel de administración.
export class AdminService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = `${environment.apiUrl}Admin`;

  // Pide la lista de usuarios al backend.
  getUsers(): Observable<AdminUser[]> {
    const token = this.authService.getToken();
    const body = { AdminToken: token };
    return this.http.post<AdminUser[]>(`${this.apiUrl}/users`, body);
  }

  // Envía los datos para actualizar un usuario.
  updateUser(userId: number, userData: AdminUpdateUser): Observable<any> {
    const token = this.authService.getToken();
    const body = { ...userData, AdminToken: token };
    return this.http.put(`${this.apiUrl}/users/${userId}`, body);
  }

  // Pide al backend que elimine un usuario.
  deleteUser(userId: number): Observable<any> {
    const token = this.authService.getToken();
    const body = { AdminToken: token };
    return this.http.post(`${this.apiUrl}/users/delete/${userId}`, body);
  }
}