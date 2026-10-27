import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../services/auth';
import { Notifications } from '../notifications/notifications';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, Notifications],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
  
})
export class Navbar {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly pending = signal(false);
  readonly error = signal('');

  logout(): void {
    if (this.pending()) return;
    this.pending.set(true);
    this.error.set('');
    this.auth.logout().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.pending.set(false); void this.router.navigate(['/']); },
      error: () => { this.pending.set(false); this.error.set('Çıkış yapılamadı. Tekrar deneyin.'); }
    });
  }
}
