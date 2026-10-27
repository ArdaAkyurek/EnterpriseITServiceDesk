import { Component, DestroyRef, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  readonly pending = signal(false);
  readonly error = signal('');
  private readonly sessionReason = inject(ActivatedRoute).snapshot.queryParamMap.get('reason');
  readonly sessionMessage = this.sessionReason === 'password-changed'
    ? 'Parolanız değiştirildi. Yeni parolanızla giriş yapın.'
    : this.sessionReason ? 'Oturumunuzu doğrulayamadık. Lütfen tekrar giriş yapın.' : '';
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    password: ['', [Validators.required, Validators.maxLength(128)]]
  });

  login(): void {
    if (this.pending()) return;
    this.error.set('');
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.pending.set(true);
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.pending.set(false);
        this.form.controls.password.reset();
        void this.router.navigate(['/dashboard']);
      },
      error: (error: HttpErrorResponse) => {
        this.pending.set(false);
        this.form.controls.password.reset();
        this.error.set(error.status === 401 ? 'E-posta veya parola hatalı.'
          : error.status === 429 ? 'Çok fazla giriş denemesi yaptınız. Bir dakika sonra tekrar deneyin.'
          : 'Giriş yapılamadı. Bağlantınızı kontrol edip tekrar deneyin.');
      }
    });
  }

}
