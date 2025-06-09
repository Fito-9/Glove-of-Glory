import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { AuthService } from '../../services/authservice';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
   // Propiedades para vincular con el formulario
   nombreUsuario = '';
   email = ''; 
   password = ''; 
   selectedFile: File | null = null;
   errorMessage: string | null = null;
 
   private authService = inject(AuthService);
   private router = inject(Router);
 
   // Método para capturar el archivo seleccionado, igual que en tu ejemplo
   onFileSelected(event: any) {
     const file: File = event.target.files[0];
     if (file) {
       this.selectedFile = file;
     }
   }
 
   async onSubmit() {
     // Creamos el objeto FormData para enviar los datos, incluyendo el archivo
     const formData = new FormData();
     // Usamos los nombres de campo que espera el backend .NET
     formData.append('NombreUsuario', this.nombreUsuario);
     formData.append('Email', this.email);
     formData.append('Password', this.password);
     if (this.selectedFile) {
       formData.append('Imagen', this.selectedFile, this.selectedFile.name);
     }
 
     try {
       // Usamos lastValueFrom para el estilo async/await
       const result = await lastValueFrom(this.authService.register(formData));
       if (result) {
         alert('¡Registro completado! Ahora inicia sesión.');
         this.router.navigate(['/login']); 
       }
     } catch (error) {
       console.error('Error en el registro:', error);
       if (error instanceof HttpErrorResponse) {
         this.errorMessage = error.error?.message || 'Error en el registro. Inténtalo de nuevo.';
       } else {
         this.errorMessage = 'Ocurrió un error inesperado durante el registro.';
       }
     }
   }
}
