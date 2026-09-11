import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  template: `
    <main class="auth-layout">
      <section class="auth-story">
        <div class="brand"><span>H</span> HelpDesk <b>AI</b></div>
        <div class="story-copy"><p class="eyebrow">A clearer support queue</p><h1>Resolve the right conversation, at the right moment.</h1><p>One workspace for team workload, customer context, and AI-assisted replies.</p></div>
        <div class="signal"><strong>6</strong><span>seeded conversations ready for triage</span></div>
      </section>
      <section class="form-panel">
        <mat-card appearance="outlined">
          <p class="eyebrow">{{ isRegister ? 'Create workspace access' : 'Welcome back' }}</p>
          <h2>{{ isRegister ? 'Join the support team' : 'Sign in to your queue' }}</h2>
          <form [formGroup]="form" (ngSubmit)="submit()">
            @if (isRegister) { <mat-form-field appearance="outline"><mat-label>Full name</mat-label><input matInput formControlName="name" autocomplete="name"></mat-form-field> }
            <mat-form-field appearance="outline"><mat-label>Email</mat-label><input matInput type="email" formControlName="email" autocomplete="email"><mat-error>Enter a valid email.</mat-error></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>Password</mat-label><input matInput type="password" formControlName="password" autocomplete="current-password"><mat-hint>At least 8 characters, an uppercase letter, and a number</mat-hint></mat-form-field>
            <button mat-flat-button type="submit" [disabled]="form.invalid || loading()">@if (loading()) { <mat-spinner diameter="20" /> } @else { {{ isRegister ? 'Create account' : 'Sign in' }} }</button>
          </form>
          <p class="switch">{{ isRegister ? 'Already have access?' : 'New to HelpDesk AI?' }} <a [routerLink]="isRegister ? '/login' : '/register'">{{ isRegister ? 'Sign in' : 'Create an account' }}</a></p>
          @if (!isRegister) { <div class="demo"><span>Demo admin</span><button type="button" (click)="fillDemo()">admin&#64;helpdesk.local · Admin123!</button></div> }
        </mat-card>
      </section>
    </main>
  `,
  styles: [`
    .auth-layout{min-height:100vh;display:grid;grid-template-columns:minmax(360px,44%) 1fr;background:#f4f7fb}.auth-story{position:relative;display:flex;flex-direction:column;justify-content:space-between;padding:3rem;color:#fff;background:radial-gradient(circle at 85% 15%,#375fb7 0,transparent 30%),linear-gradient(145deg,#10192b,#1b2b4b)}.brand{font-size:1.15rem}.brand span{display:inline-grid;place-items:center;width:2rem;height:2rem;margin-right:.5rem;border-radius:.6rem;background:#4d83ef;font-weight:800}.brand b{color:#7cafff}.story-copy{max-width:560px}.story-copy h1{font-size:clamp(2.2rem,4vw,4.4rem);line-height:1.03;letter-spacing:-.055em;margin:.8rem 0 1.25rem}.story-copy>p:last-child{max-width:440px;color:#bdc9dd;font-size:1.05rem;line-height:1.7}.eyebrow{text-transform:uppercase;letter-spacing:.12em;font-size:.72rem;font-weight:800;color:#6e9eff}.signal{display:flex;align-items:center;gap:1rem;color:#aab8d0}.signal strong{font-size:2.4rem;color:#fff}.signal span{max-width:200px;font-size:.82rem}.form-panel{display:grid;place-items:center;padding:2rem}mat-card{width:min(450px,100%);padding:2.4rem;border-color:#dde4ee!important;box-shadow:0 20px 60px rgba(19,35,63,.09)}h2{font-size:1.8rem;letter-spacing:-.03em;margin:.35rem 0 1.8rem}form{display:grid;gap:.4rem}form>button{height:48px;margin-top:.6rem}mat-spinner{margin:auto}.switch{text-align:center;margin:1.6rem 0 .8rem;color:#667085}.switch a{color:#2867d9;font-weight:700}.demo{border-top:1px solid #e6eaf0;padding-top:1rem;display:grid;text-align:center;color:#7b8595;font-size:.78rem}.demo button{border:0;background:none;color:#2f64ba;cursor:pointer}@media(max-width:800px){.auth-layout{grid-template-columns:1fr}.auth-story{display:none}.form-panel{padding:1rem}}
  `]
})
export class AuthPage {
  private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService); private readonly router = inject(Router); private readonly route = inject(ActivatedRoute);
  readonly loading = signal(false); readonly isRegister = this.route.snapshot.data['mode'] === 'register';
  readonly form = this.fb.nonNullable.group({ name: ['', this.isRegister ? [Validators.required, Validators.maxLength(100)] : []], email: ['', [Validators.required, Validators.email]], password: ['', [Validators.required, Validators.minLength(8)]] });
  fillDemo(): void { this.form.patchValue({ email: 'admin@helpdesk.local', password: 'Admin123!' }); }
  submit(): void { if (this.form.invalid) return; this.loading.set(true); const {name,email,password}=this.form.getRawValue(); const request=this.isRegister?this.auth.register(name,email,password):this.auth.login(email,password); request.pipe(finalize(()=>this.loading.set(false))).subscribe(()=>void this.router.navigateByUrl(this.route.snapshot.queryParamMap.get('returnUrl')||'/dashboard')); }
}
