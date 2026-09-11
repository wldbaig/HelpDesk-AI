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
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss',
})
export class AuthPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly isRegister = this.route.snapshot.data['mode'] === 'register';

  readonly form = this.fb.nonNullable.group({
    name: ['', this.isRegister ? [Validators.required, Validators.maxLength(100)] : []],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  fillDemo(): void {
    this.form.patchValue({ email: 'admin@helpdesk.local', password: 'Admin123!' });
  }

  submit(): void {
    if (this.form.invalid) return;

    this.loading.set(true);
    const { name, email, password } = this.form.getRawValue();
    const request = this.isRegister
      ? this.auth.register(name, email, password)
      : this.auth.login(email, password);

    request.pipe(finalize(() => this.loading.set(false))).subscribe(() => {
      const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/dashboard';
      void this.router.navigateByUrl(returnUrl);
    });
  }
}
