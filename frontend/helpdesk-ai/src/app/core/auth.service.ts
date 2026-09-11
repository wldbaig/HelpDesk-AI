import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { AuthResponse, User } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly userState = signal<User | null>(this.readUser());
  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => !!this.token() && !!this.userState());
  login(email: string, password: string): Observable<AuthResponse> { return this.api.login({ email, password }).pipe(tap(result => this.save(result))); }
  register(name: string, email: string, password: string): Observable<AuthResponse> { return this.api.register({ name, email, password }).pipe(tap(result => this.save(result))); }
  token(): string | null { return sessionStorage.getItem('helpdesk_token'); }
  logout(): void { sessionStorage.removeItem('helpdesk_token'); sessionStorage.removeItem('helpdesk_user'); this.userState.set(null); }
  private save(result: AuthResponse): void { sessionStorage.setItem('helpdesk_token', result.token); sessionStorage.setItem('helpdesk_user', JSON.stringify(result.user)); this.userState.set(result.user); }
  private readUser(): User | null { try { return JSON.parse(sessionStorage.getItem('helpdesk_user') ?? 'null'); } catch { return null; } }
}
