import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../services/auth';
import { UserProfile } from '../../models/auth';

@Component({
  selector: 'app-profile', standalone: true, imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './profile.html', styleUrl: './profile.css'
})
export class Profile implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  readonly profile = signal<UserProfile | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly savingPhone = signal(false);
  readonly savingPassword = signal(false);
  readonly phoneMessage = signal('');
  readonly phoneError = signal('');
  readonly passwordError = signal('');
  readonly initials = computed(() => {
    const user = this.profile();
    return `${user?.firstName.charAt(0) ?? ''}${user?.lastName.charAt(0) ?? ''}`.toLocaleUpperCase('tr-TR');
  });
  readonly roleLabel = computed(() => ({ Admin: 'Yönetici', Technician: 'Teknisyen', Employee: 'Çalışan' })[this.profile()?.role ?? 'Employee']);
  readonly phoneForm = this.fb.nonNullable.group({ phone: ['', Validators.maxLength(20)] });
  readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', [Validators.required, Validators.maxLength(128)]],
    newPassword: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]],
    confirmPassword: ['', Validators.required]
  }, { validators: control => {
    const { currentPassword, newPassword, confirmPassword } = control.value;
    if (newPassword && currentPassword === newPassword) return { samePassword: true };
    return confirmPassword && newPassword !== confirmPassword ? { passwordMismatch: true } : null;
  }});

  ngOnInit(): void { this.loadProfile(); }

  loadProfile(): void {
    if (this.loading() || this.savingPhone() || this.savingPassword()) return;
    this.loading.set(true); this.error.set('');
    this.auth.getProfile().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: profile => {
        this.profile.set(profile); this.phoneForm.reset({ phone: profile.phone ?? '' }); this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.error.set('Profiliniz yüklenemedi. Tekrar deneyin.'); }
    });
  }

  savePhone(): void {
    if (this.savingPhone() || this.savingPassword() || !this.profile()) return;
    this.phoneForm.markAllAsTouched(); this.phoneError.set(''); this.phoneMessage.set('');
    if (this.phoneForm.invalid) return;
    this.savingPhone.set(true);
    this.auth.updateProfile(this.phoneForm.getRawValue().phone.trim() || null)
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: profile => {
          this.profile.set(profile); this.phoneForm.reset({ phone: profile.phone ?? '' });
          this.savingPhone.set(false); this.phoneMessage.set('Telefon bilginiz güncellendi.');
        },
        error: () => { this.savingPhone.set(false); this.phoneError.set('Telefon bilginiz kaydedilemedi. Tekrar deneyin.'); }
      });
  }

  changePassword(): void {
    if (this.savingPhone() || this.savingPassword() || !this.profile()) return;
    this.passwordForm.markAllAsTouched(); this.passwordError.set('');
    if (this.passwordForm.invalid) return;
    this.savingPassword.set(true);
    this.auth.changePassword(this.passwordForm.getRawValue()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.passwordForm.reset(); this.savingPassword.set(false);
        void this.router.navigate(['/'], { queryParams: { reason: 'password-changed' } });
      },
      error: (response: HttpErrorResponse) => {
        this.passwordForm.reset(); this.savingPassword.set(false);
        this.passwordError.set(response.status === 429 ? 'Çok fazla deneme yaptınız. Bir dakika sonra tekrar deneyin.'
          : typeof response.error?.message === 'string' && [400, 409].includes(response.status)
            ? response.error.message : 'Parola değiştirilemedi. Bağlantınızı kontrol edip tekrar deneyin.');
      }
    });
  }
}
