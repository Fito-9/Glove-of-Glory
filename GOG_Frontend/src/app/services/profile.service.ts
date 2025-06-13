import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './authservice';

export interface UserProfile {
  nickname: string;
  email: string;
  puntuacionElo: number;
  avatarUrl: string;
}

@Injectable({
  providedIn: 'root'
})
// Servicio para manejar el perfil del usuario.
export class ProfileService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = `${environment.apiUrl}Profile`;

  // Pide los datos del perfil al backend.
  getUserProfile(): Observable<UserProfile> {
    const token = this.authService.getToken();
    const body = { AdminToken: token }; // El backend espera el token en el body.
    return this.http.post<UserProfile>(this.apiUrl, body);
  }

  // Envía los datos actualizados del perfil.
  updateUserProfile(formData: FormData): Observable<any> {
    const token = this.authService.getToken();
    formData.append('AdminToken', token || ''); // Añadimos el token al FormData.
    return this.http.put(this.apiUrl, formData);
  }
}