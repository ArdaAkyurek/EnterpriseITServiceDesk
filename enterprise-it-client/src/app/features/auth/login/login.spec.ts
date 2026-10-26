import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Login } from './login';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../../../services/auth';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  afterEach(() => { http.verify(); TestBed.inject(AuthService).clearSession(); });

  it('does not send invalid credentials', () => {
    component.login();
    http.expectNone('/api/Auth/login');
    expect(component.form.touched).toBe(true);
  });

  it('logs in, clears the password and navigates to the dashboard', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    component.form.setValue({ email: 'admin@example.com', password: 'Valid-Test-Password' });
    component.login();
    http.expectOne('/api/Auth/login').flush({
      accessToken: 'test-token', expiresAt: new Date(Date.now() + 60000).toISOString(),
      user: { userId: 1, firstName: 'Test', lastName: 'Admin', email: 'admin@example.com', role: 'Admin' }
    });
    expect(component.form.controls.password.value).toBe('');
    expect(navigate).toHaveBeenCalledWith(['/dashboard']);
    expect(TestBed.inject(AuthService).user()?.role).toBe('Admin');
  });

  it('shows a generic error without retaining the rejected password', () => {
    component.form.setValue({ email: 'admin@example.com', password: 'Wrong-Password' });
    component.login();
    http.expectOne('/api/Auth/login').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(component.error()).toBe('E-posta veya parola hatalı.');
    expect(component.form.controls.password.value).toBe('');
    expect(component.pending()).toBe(false);
  });
});
