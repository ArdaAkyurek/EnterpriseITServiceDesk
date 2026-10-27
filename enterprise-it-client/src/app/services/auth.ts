import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { AuthUser, ChangePassword, LoginResponse, UserProfile, UserRole } from '../models/auth';

type StoredSession = Pick<LoginResponse, 'accessToken' | 'expiresAt'>;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storageKey = 'enterprise-it-session';
  private session: StoredSession | null = null;
  private expiryTimer?: ReturnType<typeof setTimeout>;
  readonly user = signal<AuthUser | null>(null);
  readonly permissionError = signal('');

  constructor() {
    try {
      const stored = JSON.parse(sessionStorage.getItem(this.storageKey) ?? 'null');
      if (typeof stored?.accessToken === 'string' && Date.parse(stored.expiresAt) > Date.now()) {
        this.session = stored;
        this.scheduleExpiry();
      } else {
        sessionStorage.removeItem(this.storageKey);
      }
    } catch { sessionStorage.removeItem(this.storageKey); }
  }

  get token(): string | null {
    return this.session && Date.parse(this.session.expiresAt) > Date.now() ? this.session.accessToken : null;
  }

  hasRole(...roles: UserRole[]): boolean {
    const user = this.user();
    return !!user && roles.includes(user.role);
  }

  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/Auth/login', { email: email.trim(), password }).pipe(
      tap(response => {
        this.session = { accessToken: response.accessToken, expiresAt: response.expiresAt };
        sessionStorage.setItem(this.storageKey, JSON.stringify(this.session));
        this.user.set(response.user);
        this.permissionError.set('');
        this.scheduleExpiry();
      })
    );
  }

  loadUser() {
    return this.http.get<AuthUser>('/api/Auth/me').pipe(tap(user => this.user.set(user)));
  }

  logout() {
    return this.http.post<void>('/api/Auth/logout', {}).pipe(tap(() => this.clearSession()));
  }

  getProfile() { return this.http.get<UserProfile>('/api/Auth/profile'); }

  updateProfile(phone: string | null) {
    return this.http.put<UserProfile>('/api/Auth/profile', { phone });
  }

  changePassword(passwords: ChangePassword) {
    return this.http.put<void>('/api/Auth/password', passwords).pipe(tap(() => this.clearSession()));
  }

  clearSession(): void {
    clearTimeout(this.expiryTimer);
    this.session = null;
    this.user.set(null);
    this.permissionError.set('');
    sessionStorage.removeItem(this.storageKey);
  }

  private scheduleExpiry(): void {
    clearTimeout(this.expiryTimer);
    this.expiryTimer = setTimeout(() => {
      this.clearSession();
      void this.router.navigate(['/'], { queryParams: { reason: 'expired' } });
    }, Math.max(0, Date.parse(this.session!.expiresAt) - Date.now()));
  }
}
