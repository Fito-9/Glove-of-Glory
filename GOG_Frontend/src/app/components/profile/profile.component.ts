import { Component, inject, OnInit } from '@angular/core';
import { ProfileService, UserProfile } from '../../services/profile.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/authservice';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  private authService = inject(AuthService);

  userProfile: UserProfile | null = null;
  isLoading = true;
  error: string | null = null;

  // Propiedades para el formulario de edición
  editingNickname: string = '';
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.profileService.getUserProfile().subscribe({
      next: (data) => {
        this.userProfile = data;
        this.editingNickname = data.nickname;
        this.previewUrl = data.avatarUrl;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'No se pudo cargar el perfil.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result;
      };
      reader.readAsDataURL(file);
    }
  }

  async onSubmit(): Promise<void> {
    const formData = new FormData();
    formData.append('Nickname', this.editingNickname);
    if (this.selectedFile) {
      formData.append('Image', this.selectedFile, this.selectedFile.name);
    }

    try {
      await lastValueFrom(this.profileService.updateUserProfile(formData));
      alert('Perfil actualizado con éxito. Los cambios se reflejarán la próxima vez que inicies sesión.');
      this.loadProfile();
    } catch (err) {
      alert('Error al actualizar el perfil.');
      console.error(err);
    }
  }
}