import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { MatchRoomComponent } from './components/match-room/match-room.component';
import { FriendshipComponent } from './components/friendship/friendship.component';
import { RankingComponent } from './components/ranking/ranking.component';
import { AdminPanelComponent } from './components/admin-panel/admin-panel.component';

export const routes: Routes = [
  { 
    path: '', 
    component: HomeComponent, 
  },
  { 
    path: 'login', 
    component: LoginComponent 
  },
  { 
    path: 'register', 
    component: RegisterComponent 
  },
  { path: 'match/:roomId', component: MatchRoomComponent },
  { path: 'friends', component: FriendshipComponent },
  { path: 'ranking', component: RankingComponent },
  { path: 'admin', component: AdminPanelComponent },
  { 
    path: '**', 
    redirectTo: 'login' 
  }
];