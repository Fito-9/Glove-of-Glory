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
// El formulario de loginn
export class LoginComponent {
   email = ''; 
   password = ''; 
   errorMessage: string | null = null;
 
   private authService = inject(AuthService);
   private router = inject(Router);
 
   async onSubmit() {
     const credentials = { Email: this.email, Password: this.password }; 
 
     try {
       const result = await lastValueFrom(this.authService.login(credentials));
 
       if (result && result.accessToken) {
         console.log("Login correcto.");
         this.router.navigate(['/']);
       } else {
         this.errorMessage = "Respuesta rara del servidor.";
       }
     } catch (error) {
       console.error("Error al iniciar sesión:", error);
       if (error instanceof HttpErrorResponse) {
         this.errorMessage = error.error?.message || 'Error en el login. Revisa tus datos.';
       } else {
         this.errorMessage = 'Error inesperado.';
       }
     }
   }
}