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
export class AdminService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = `${environment.apiUrl}Admin`;

  // ✅ CAMBIO: Ya no necesitamos cabeceras, el token va en el body

  getUsers(): Observable<AdminUser[]> {
    const token = this.authService.getToken();
    const body = { AdminToken: token };
    // ✅ CAMBIO: Ahora es un POST que envía el token en el cuerpo
    return this.http.post<AdminUser[]>(`${this.apiUrl}/users`, body);
  }

  updateUser(userId: number, userData: AdminUpdateUser): Observable<any> {
    const token = this.authService.getToken();
    // ✅ CAMBIO: Añadimos el token al cuerpo de la petición
    const body = { ...userData, AdminToken: token };
    return this.http.put(`${this.apiUrl}/users/${userId}`, body);
  }

  deleteUser(userId: number): Observable<any> {
    const token = this.authService.getToken();
    const body = { AdminToken: token };
    // ✅ CAMBIO: Ahora es un POST para poder enviar el token en el cuerpo
    return this.http.post(`${this.apiUrl}/users/delete/${userId}`, body);
  }
}