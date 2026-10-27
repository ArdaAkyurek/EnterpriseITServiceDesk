import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { Profile } from './profile';
import { AuthService } from '../../services/auth';

describe('Profile', () => {
  let fixture: ComponentFixture<Profile>;
  let http: HttpTestingController;
  const profile = { userId: 3, firstName: 'Test', lastName: 'Employee', email: 'test@example.com', role: 'Employee',
    department: 'IT', phone: null, isActive: true, createdAt: '2026-10-26T09:00:00Z' };

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Profile], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] }).compileComponents();
    fixture = TestBed.createComponent(Profile);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });
  afterEach(() => { http.verify(); TestBed.inject(AuthService).clearSession(); });
  function load(): void { http.expectOne('/api/Auth/profile').flush(profile); fixture.detectChanges(); }
  function passwords(): void { fixture.componentInstance.passwordForm.setValue({ currentPassword: 'Current-Password-123', newPassword: 'New-Strong-Password-123', confirmPassword: 'New-Strong-Password-123' }); }

  it('shows identity and account fields without role or email editing', () => {
    load();
    expect(fixture.nativeElement.textContent).toContain('Test Employee');
    expect(fixture.nativeElement.textContent).toContain('Çalışan');
    expect(fixture.nativeElement.textContent).toContain('IT');
    expect(fixture.nativeElement.querySelector('input[type="email"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('select')).toBeNull();
  });

  it('sends only the phone field and shows a saved message', () => {
    load();
    fixture.componentInstance.phoneForm.controls.phone.setValue(' +90 555 123 45 67 ');
    fixture.componentInstance.savePhone();
    const request = http.expectOne('/api/Auth/profile');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ phone: '+90 555 123 45 67' });
    request.flush({ ...profile, phone: '+90 555 123 45 67' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Telefon bilginiz güncellendi.');
  });

  it('allows clearing the phone but rejects overlong input', () => {
    load();
    fixture.componentInstance.phoneForm.controls.phone.setValue('1'.repeat(21));
    fixture.componentInstance.savePhone();
    http.expectNone('/api/Auth/profile');
    fixture.componentInstance.phoneForm.controls.phone.setValue('');
    fixture.componentInstance.savePhone();
    const request = http.expectOne('/api/Auth/profile');
    expect(request.request.body).toEqual({ phone: null });
    request.flush(profile);
  });

  it('blocks mismatched and reused passwords before sending', () => {
    load(); passwords();
    fixture.componentInstance.passwordForm.controls.confirmPassword.setValue('different');
    fixture.componentInstance.changePassword();
    http.expectNone('/api/Auth/password');
    expect(fixture.componentInstance.passwordForm.hasError('passwordMismatch')).toBe(true);
    fixture.componentInstance.passwordForm.patchValue({ newPassword: 'Current-Password-123', confirmPassword: 'Current-Password-123' });
    fixture.componentInstance.changePassword();
    http.expectNone('/api/Auth/password');
    expect(fixture.componentInstance.passwordForm.hasError('samePassword')).toBe(true);
  });

  it('clears the session and password inputs after a successful change', () => {
    load(); passwords();
    const auth = TestBed.inject(AuthService);
    auth.user.set({ ...profile, role: 'Employee' });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.componentInstance.changePassword();
    const request = http.expectOne('/api/Auth/password');
    expect(request.request.method).toBe('PUT');
    fixture.componentInstance.changePassword();
    http.expectNone('/api/Auth/password');
    request.flush(null);
    expect(auth.user()).toBeNull();
    expect(fixture.componentInstance.passwordForm.controls.currentPassword.value).toBe('');
    expect(navigate).toHaveBeenCalledWith(['/'], { queryParams: { reason: 'password-changed' } });
  });

  it('shows incorrect-current-password errors without logging the user out', () => {
    load(); passwords();
    const auth = TestBed.inject(AuthService);
    auth.user.set({ ...profile, role: 'Employee' });
    fixture.componentInstance.changePassword();
    http.expectOne('/api/Auth/password').flush({ message: 'Mevcut parolanız hatalı.' }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Mevcut parolanız hatalı.');
    expect(auth.user()).not.toBeNull();
    expect(fixture.componentInstance.passwordForm.controls.newPassword.value).toBe('');
  });

  it('retries loading and displays phone save failures', () => {
    http.expectOne('/api/Auth/profile').flush({}, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click();
    load();
    fixture.componentInstance.savePhone();
    http.expectOne('/api/Auth/profile').flush({}, { status: 503, statusText: 'Unavailable' });
    expect(fixture.componentInstance.phoneError()).toContain('kaydedilemedi');
    expect(fixture.componentInstance.savingPhone()).toBe(false);
  });

  it('prevents simultaneous phone and password submissions', () => {
    load(); passwords();
    fixture.componentInstance.savePhone();
    fixture.componentInstance.changePassword();
    http.expectNone('/api/Auth/password');
    http.expectOne('/api/Auth/profile').flush(profile);
  });
});
