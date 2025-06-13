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
export class ProfileService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = `${environment.apiUrl}Profile`;

  getUserProfile(): Observable<UserProfile> {
    const token = this.authService.getToken();
    const body = { AdminToken: token };
    return this.http.post<UserProfile>(this.apiUrl, body);
  }

  updateUserProfile(formData: FormData): Observable<any> {
    const token = this.authService.getToken();
    formData.append('AdminToken', token || '');
    return this.http.put(this.apiUrl, formData);
  }
}