import { Component, inject, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CommonModule } from '@angular/common';

export interface RankingUser {
  userId: number;
  nickname: string;
  ruta: string;
  puntuacionElo: number;
}

@Component({
  selector: 'app-ranking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ranking.component.html',
  styleUrls: ['./ranking.component.css']
})
export class RankingComponent implements OnInit {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}User/ranking`;

  ranking: RankingUser[] = [];
  isLoading = true;
  error: string | null = null;

  ngOnInit(): void {
    this.loadRanking();
  }

  loadRanking(): void {
    this.isLoading = true;
    this.http.get<RankingUser[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.ranking = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error al cargar el ranking:', err);
        this.error = 'No se pudo cargar la clasificación. Inténtalo de nuevo más tarde.';
        this.isLoading = false;
      }
    });
  }
}