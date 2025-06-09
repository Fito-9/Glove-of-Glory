import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

export interface AuthResponse {
  accessToken: string;
  usuarioId: number;
  avatar: string;
  nombreUsuario: string; // Asumimos que el backend lo devuelve o lo guardamos nosotros
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7096/api/User';
  
  // Usamos signals para reactividad en el estado de autenticación
  public currentUserSig = signal<AuthResponse | undefined | null>(undefined);

  constructor(private http: HttpClient, private router: Router) {
    // Al iniciar el servicio, intentamos cargar los datos del usuario desde localStorage
    const user_data = localStorage.getItem('user_data');
    if (user_data) {
      this.currentUserSig.set(JSON.parse(user_data));
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
        // Guardamos el token y los datos del usuario
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
}