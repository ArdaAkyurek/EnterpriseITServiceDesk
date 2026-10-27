import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth';
import { authInterceptor } from './auth.interceptor';
import { authGuard } from './auth.guard';
import { routes } from '../app.routes';

describe('Authentication', () => {
  let auth: AuthService;
  let http: HttpTestingController;
  const user = { userId: 1, firstName: 'Test', lastName: 'Admin', email: 'test@example.com', role: 'Admin' };

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [
      provideRouter([]), provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()
    ] });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });
  afterEach(() => { http.verify(); auth.clearSession(); });

  function login(): void {
    auth.login('test@example.com', 'Test-Password').subscribe();
    const request = http.expectOne('/api/Auth/login');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({ accessToken: 'signed-token', expiresAt: new Date(Date.now() + 60000).toISOString(), user });
  }

  it('attaches the bearer only to local API requests', () => {
    login();
    TestBed.inject(HttpClient).get('/api/Asset').subscribe();
    const local = http.expectOne('/api/Asset');
    expect(local.request.headers.get('Authorization')).toBe('Bearer signed-token');
    local.flush([]);
    TestBed.inject(HttpClient).get('https://example.com/data').subscribe();
    const external = http.expectOne('https://example.com/data');
    expect(external.request.headers.has('Authorization')).toBe(false);
    external.flush({});
  });

  it('revokes the server session before clearing browser storage on logout', () => {
    login();
    auth.logout().subscribe();
    const request = http.expectOne('/api/Auth/logout');
    expect(request.request.headers.get('Authorization')).toBe('Bearer signed-token');
    request.flush(null);
    expect(auth.token).toBeNull();
    expect(auth.user()).toBeNull();
    expect(sessionStorage.getItem('enterprise-it-session')).toBeNull();
  });

  it('clears invalid sessions and redirects on 401', () => {
    login();
    auth.loadUser().subscribe({ error: () => {} });
    http.expectOne('/api/Auth/me').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(auth.token).toBeNull();
    expect(TestBed.inject(Router).navigate).toHaveBeenCalled();
  });

  it('blocks protected navigation without a session', () => {
    const result = TestBed.runInInjectionContext(() => authGuard({ data: {} } as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    expect(result instanceof UrlTree).toBe(true);
    http.expectNone('/api/Auth/me');
  });

  it('checks the server role and redirects employees away from assets', () => {
    login();
    const result = TestBed.runInInjectionContext(() => authGuard({ data: { roles: ['Admin', 'Technician'] } } as unknown as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    let redirect: unknown;
    (result as Observable<unknown>).subscribe(value => redirect = value);
    http.expectOne('/api/Auth/me').flush({ ...user, role: 'Employee' });
    expect(redirect instanceof UrlTree).toBe(true);
    expect(String(redirect)).toBe('/dashboard');
    expect(auth.hasRole('Admin', 'Technician')).toBe(false);
  });

  it.each(['Admin', 'Technician', 'Employee'])('enforces the Roles route for %s', role => {
    login();
    const roleRoute = routes.find(route => route.path === 'dashboard')!.children!.find(route => route.path === 'roles')!;
    expect(roleRoute.data?.['roles']).toEqual(['Admin']);
    const result = TestBed.runInInjectionContext(() => authGuard({ data: roleRoute.data } as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    let decision: unknown;
    (result as Observable<unknown>).subscribe(value => decision = value);
    http.expectOne('/api/Auth/me').flush({ ...user, role });
    expect(role === 'Admin' ? decision === true : String(decision) === '/dashboard').toBe(true);
  });

  it.each(['Admin', 'Technician', 'Employee'])('enforces the Departments route for %s', role => {
    login();
    const departmentRoute = routes.find(route => route.path === 'dashboard')!.children!.find(route => route.path === 'departments')!;
    expect(departmentRoute.data?.['roles']).toEqual(['Admin']);
    const result = TestBed.runInInjectionContext(() => authGuard({ data: departmentRoute.data } as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    let decision: unknown;
    (result as Observable<unknown>).subscribe(value => decision = value);
    http.expectOne('/api/Auth/me').flush({ ...user, role });
    expect(role === 'Admin' ? decision === true : String(decision) === '/dashboard').toBe(true);
  });

  it.each(['Admin', 'Technician', 'Employee'])('enforces the Audit route for %s', role => {
    login();
    const auditRoute = routes.find(route => route.path === 'dashboard')!.children!.find(route => route.path === 'audit-logs')!;
    expect(auditRoute.data?.['roles']).toEqual(['Admin']);
    const result = TestBed.runInInjectionContext(() => authGuard({ data: auditRoute.data } as ActivatedRouteSnapshot, {} as RouterStateSnapshot));
    let decision: unknown;
    (result as Observable<unknown>).subscribe(value => decision = value);
    http.expectOne('/api/Auth/me').flush({ ...user, role });
    expect(role === 'Admin' ? decision === true : String(decision) === '/dashboard').toBe(true);
  });
});
