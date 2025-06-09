import { Component, inject } from '@angular/core';
import { AuthService } from '../../services/authservice';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
   // Propiedades para vincular con el formulario usando ngModel
   email = ''; 
   password = ''; 
   errorMessage: string | null = null;
 
   // Inyectamos los servicios necesarios
   private authService = inject(AuthService);
   private router = inject(Router);
 
   async onSubmit() {
     // Creamos el objeto con los datos para el backend
     const credentials = { Email: this.email, Password: this.password }; 
 
     try {
       // Usamos lastValueFrom para convertir el Observable en una Promesa (estilo async/await)
       const result = await lastValueFrom(this.authService.login(credentials));
 
       if (result && result.accessToken) {
         console.log("Inicio de sesión exitoso.");
         // La redirección se maneja dentro del servicio, pero la reforzamos aquí
         this.router.navigate(['/']);
       } else {
         // Este caso es poco probable si el backend siempre devuelve un token en éxito
         this.errorMessage = "No se recibió una respuesta válida del servidor.";
       }
     } catch (error) {
       console.error("Error al iniciar sesión:", error);
       if (error instanceof HttpErrorResponse) {
         // Mostramos el mensaje de error que viene del backend
         this.errorMessage = error.error?.message || 'Error en el inicio de sesión. Verifica tus credenciales.';
       } else {
         this.errorMessage = 'Ocurrió un error inesperado.';
       }
     }
   }
}
