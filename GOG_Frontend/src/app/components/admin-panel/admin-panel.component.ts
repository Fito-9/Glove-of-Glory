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
export class AdminPanelComponent implements OnInit {
  private adminService = inject(AdminService);

  users: AdminUser[] = [];
  isLoading = true;
  error: string | null = null;

  editingUser: AdminUser | null = null;
  
  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.adminService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error al cargar usuarios:', err);
        this.error = 'No se pudo cargar la lista de usuarios. ¿Tienes permisos de administrador?';
        this.isLoading = false;
      }
    });
  }

  editUser(user: AdminUser): void {
    // Creamos una copia para no modificar el original hasta guardar
    this.editingUser = { ...user };
  }

  saveUser(): void {
    if (!this.editingUser) return;

    const { userId, nickname, puntuacionElo, rol } = this.editingUser;
    this.adminService.updateUser(userId, { nickname, puntuacionElo, rol }).subscribe({
      next: () => {
        alert('Usuario actualizado con éxito.');
        this.editingUser = null;
        this.loadUsers(); // Recargar la lista
      },
      error: (err) => {
        alert(`Error al actualizar: ${err.error?.message || 'Error desconocido'}`);
      }
    });
  }

  cancelEdit(): void {
    this.editingUser = null;
  }

  deleteUser(userId: number, nickname: string): void {
    if (confirm(`¿Estás seguro de que quieres eliminar al usuario "${nickname}"? Esta acción es irreversible.`)) {
      this.adminService.deleteUser(userId).subscribe({
        next: () => {
          alert('Usuario eliminado con éxito.');
          this.loadUsers(); // Recargar la lista
        },
        error: (err) => {
          alert(`Error al eliminar: ${err.error?.message || 'Error desconocido'}`);
        }
      });
    }
  }
}