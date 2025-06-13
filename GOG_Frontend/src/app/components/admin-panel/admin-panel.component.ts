import { Component, inject, OnInit } from '@angular/core';
import { AdminService, AdminUser } from '../../services/admin.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-panel.component.html',
  styleUrls: ['./admin-panel.component.css']
})
// Este es el panel para que los admins controlen a los usuarios.
export class AdminPanelComponent implements OnInit {
  private adminService = inject(AdminService);

  users: AdminUser[] = [];
  isLoading = true;
  error: string | null = null;

  // Guardamos aquí el usuario que se está editando para no liar la tabla.
  editingUser: AdminUser | null = null;
  
  ngOnInit(): void {
    this.loadUsers();
  }

  // Pide la lista de usuarios al servicio.
  loadUsers(): void {
    this.isLoading = true;
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error al cargar usuarios:', err);
        this.error = 'No se pudo cargar la lista. ¿Seguro que eres admin?';
        this.isLoading = false;
      }
    });
  }

  // Prepara un usuario para ser editado.
  editUser(user: AdminUser): void {
    // Hacemos una copia para poder cancelar sin guardar cambios.
    this.editingUser = { ...user };
  }

  // Guarda los cambios del usuario editado.
  saveUser(): void {
    if (!this.editingUser) return;

    const { userId, nickname, puntuacionElo, rol } = this.editingUser;
    this.adminService.updateUser(userId, { nickname, puntuacionElo, rol }).subscribe({
      next: () => {
        alert('Usuario actualizado.');
        this.editingUser = null;
        this.loadUsers(); // Recargamos la lista para ver los cambios.
      },
      error: (err) => {
        alert(`Error al actualizar: ${err.error?.message || 'Error desconocido'}`);
      }
    });
  }

  // Cancela la edición y vuelve a la normalidad.
  cancelEdit(): void {
    this.editingUser = null;
  }

  // Borra un usuario, previa confirmación.
  deleteUser(userId: number, nickname: string): void {
    if (confirm(`¿Seguro que quieres borrar a "${nickname}"? Esto no se puede deshacer.`)) {
      this.adminService.deleteUser(userId).subscribe({
        next: () => {
          alert('Usuario eliminado.');
          this.loadUsers();
        },
        error: (err) => {
          alert(`Error al eliminar: ${err.error?.message || 'Error desconocido'}`);
        }
      });
    }
  }
}