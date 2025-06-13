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
// Servicio para gestionar la autenticación: login, logout, registro y datos del usuario.
export class AuthService {
  private apiUrl = `${environment.apiUrl}User`;
  
  // Una "signal" para guardar los datos del usuario. Es reactiva, si cambia, la vista se entera.
  public currentUserSig = signal<AuthResponse | undefined | null>(undefined);

  constructor(private http: HttpClient, private router: Router) {
    // Al cargar la app, intentamos recuperar los datos del usuario del localStorage.
    const userData = localStorage.getItem('user_data');
    if (userData) {
      this.currentUserSig.set(JSON.parse(userData));
    } else {
      this.currentUserSig.set(null);
    }
  }

  // Llama al endpoint de registro.
  register(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, formData);
  }

  // Llama al endpoint de login.
  login(credentials: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        // Si el login es correcto, guardamos los datos.
        this.saveUserData(response);
      })
    );
  }

  // Guarda los datos del usuario en el localStorage y en la signal.
  private saveUserData(data: AuthResponse) {
    localStorage.setItem('user_data', JSON.stringify(data));
    this.currentUserSig.set(data);
  }

  // Devuelve el token del usuario actual.
  getToken(): string | null {
    const data = this.currentUserSig();
    return data ? data.accessToken : null;
  }

  // Comprueba si el usuario ha iniciado sesión.
  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  // Cierra la sesión del usuario.
  logout(): void {
    localStorage.removeItem('user_data');
    this.currentUserSig.set(null);
    this.router.navigate(['/login']);
  }

  // Actualiza el ELO del usuario en la sesión actual (sin llamar a la BD).
  updateUserElo(newElo: number): void {
    const currentUser = this.currentUserSig();
    if (currentUser) {
      const updatedUser = { ...currentUser, puntuacionElo: newElo };
      this.saveUserData(updatedUser);
      console.log(`ELO actualizado a: ${newElo}`);
    }
  }
}