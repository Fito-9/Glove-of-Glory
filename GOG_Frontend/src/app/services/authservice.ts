import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export interface AuthResponse {
  accessToken: string;
  usuarioId: number;
  nombreUsuario: string;
  puntuacionElo: number;
  avatar: string;
  rol: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}User`;
  
  public currentUserSig = signal<AuthResponse | undefined | null>(undefined);

  constructor(private http: HttpClient, private router: Router) {
    const userData = localStorage.getItem('user_data');
    if (userData) {
      this.currentUserSig.set(JSON.parse(userData));
    } else {
      this.currentUserSig.set(null);
    }
  }

  register(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, formData);
  }

  login(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        this.saveUserData(response);
      })
    );
  }

  private saveUserData(data: AuthResponse) {
    localStorage.setItem('user_data', JSON.stringify(data));
    this.currentUserSig.set(data);
  }

  getToken(): string | null {
    const data = this.currentUserSig();
    return data ? data.accessToken : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  logout(): void {
    localStorage.removeItem('user_data');
    this.currentUserSig.set(null);
    this.router.navigate(['/login']);
  }

  updateUserElo(newElo: number): void {
    const currentUser = this.currentUserSig();
    if (currentUser) {
      const updatedUser = { ...currentUser, puntuacionElo: newElo };
      this.saveUserData(updatedUser);
      console.log(`ELO actualizado a: ${newElo}`);
    }
  }
}