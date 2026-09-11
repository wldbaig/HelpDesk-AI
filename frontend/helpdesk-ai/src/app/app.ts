import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatButtonModule, MatSidenavModule, MatToolbarModule],
  template: `
    @if (auth.isAuthenticated()) {
      <mat-sidenav-container class="app-shell">
        <mat-sidenav mode="side" opened class="side-nav">
          <a class="brand" routerLink="/dashboard"><span class="brand-mark">H</span><span>HelpDesk <b>AI</b></span></a>
          <nav aria-label="Primary navigation">
            <a routerLink="/dashboard" routerLinkActive="active"><span>Overview</span><small>Live support health</small></a>
            <a routerLink="/tickets" routerLinkActive="active"><span>Tickets</span><small>Queue and conversations</small></a>
          </nav>
          <div class="profile-card"><span class="avatar">{{ auth.user()?.name?.charAt(0) }}</span><div><strong>{{ auth.user()?.name }}</strong><small>{{ auth.user()?.role }}</small></div></div>
          <button mat-button class="sign-out" (click)="logout()">Sign out</button>
        </mat-sidenav>
        <mat-sidenav-content>
          <mat-toolbar class="topbar"><span>Support operations</span><span class="spacer"></span><span class="live-dot"></span><small>Workspace online</small></mat-toolbar>
          <main class="page"><router-outlet /></main>
        </mat-sidenav-content>
      </mat-sidenav-container>
    } @else { <router-outlet /> }
  `,
  styleUrl: './app.scss',
})
export class App {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  logout(): void { this.auth.logout(); void this.router.navigateByUrl('/login'); }
}
