import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { Users } from './users';
import { AuthService } from '../../services/auth';

describe('Users', () => {
  let fixture: ComponentFixture<Users>;
  let http: HttpTestingController;
  const user = { userId: 3, firstName: 'Test', lastName: 'Employee', email: 'employee@example.com', phone: null, departmentId: 1, roleId: 3, isActive: true };
  const roles = [{ roleId: 1, roleName: 'Admin' }, { roleId: 2, roleName: 'Technician' }, { roleId: 3, roleName: 'Employee' }];
  const departments = [{ departmentId: 1, departmentName: 'IT' }];

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Users], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] }).compileComponents();
    fixture = TestBed.createComponent(Users);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });
  afterEach(() => { http.verify(); TestBed.inject(AuthService).clearSession(); });

  function load(): void {
    http.expectOne('/api/Role').flush(roles);
    http.expectOne('/api/Department').flush(departments);
    http.expectOne('/api/User').flush([user]);
    fixture.detectChanges();
  }
  function createForm(): void {
    load();
    fixture.componentInstance.openCreate();
    fixture.componentInstance.form.setValue({ firstName: ' New ', lastName: ' User ', email: 'new@example.com', phone: '', password: 'Strong-Password-123', departmentId: 1, roleId: 2, isActive: true });
    fixture.detectChanges();
  }

  it('shows user details with role and department labels', () => {
    load();
    const row = fixture.nativeElement.querySelector('tbody tr');
    expect(row.textContent).toContain('Test Employee');
    expect(row.textContent).toContain('IT');
    expect(row.textContent).toContain('Employee');
    expect(row.textContent).toContain('Aktif');
    expect(fixture.nativeElement.querySelector('input[type="password"]')).toBeNull();
  });

  it('creates a user and reloads the list without duplicate submissions', () => {
    createForm();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    const request = http.expectOne('/api/User');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.firstName).toBe('New');
    expect(request.request.body.password).toBe('Strong-Password-123');
    expect(request.request.body.phone).toBeNull();
    fixture.componentInstance.saveUser();
    http.expectNone('/api/User');
    request.flush({ ...request.request.body, userId: 4 });
    load();
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.componentInstance.form.controls.password.value).toBe('');
  });

  it('updates role and active state while omitting an unchanged password', () => {
    load();
    fixture.componentInstance.openEdit(user);
    fixture.componentInstance.form.patchValue({ roleId: 2, isActive: false });
    fixture.componentInstance.saveUser();
    const request = http.expectOne('/api/User/3');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.roleId).toBe(2);
    expect(request.request.body.isActive).toBe(false);
    expect(request.request.body.password).toBeUndefined();
    request.flush(null);
    load();
  });

  it('blocks missing fields and short passwords', () => {
    createForm();
    fixture.componentInstance.form.patchValue({ password: 'short', firstName: '   ' });
    fixture.componentInstance.saveUser();
    http.expectNone('/api/User');
    expect(fixture.componentInstance.form.invalid).toBe(true);
  });

  it('preserves the form when an email is already used', () => {
    createForm();
    fixture.componentInstance.saveUser();
    http.expectOne('/api/User').flush('Bu email zaten kullanılıyor.', { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form [role="alert"]').textContent).toContain('Bu email zaten kullanılıyor.');
    expect(fixture.componentInstance.form.controls.firstName.value).toBe(' New ');
    expect(fixture.componentInstance.saving()).toBe(false);
  });

  it('retries a failed initial load', () => {
    http.expectOne('/api/Role').flush(roles);
    http.expectOne('/api/Department').flush(departments);
    http.expectOne('/api/User').flush({}, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click();
    load();
    expect(fixture.componentInstance.error()).toBe('');
  });

  it('protects the current admin role and active state in the form', () => {
    load();
    TestBed.inject(AuthService).user.set({ userId: 3, firstName: 'Test', lastName: 'Admin', email: 'admin@example.com', role: 'Admin' });
    fixture.componentInstance.openEdit({ ...user, roleId: 1 });
    expect(fixture.componentInstance.form.controls.roleId.disabled).toBe(true);
    expect(fixture.componentInstance.form.controls.isActive.disabled).toBe(true);
  });

  it('clears the session and requests login after changing the current admin password', () => {
    load();
    const auth = TestBed.inject(AuthService);
    auth.user.set({ userId: 3, firstName: 'Test', lastName: 'Admin', email: 'admin@example.com', role: 'Admin' });
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.componentInstance.openEdit({ ...user, roleId: 1 });
    fixture.componentInstance.form.controls.password.setValue('Changed-Password-123');
    fixture.componentInstance.saveUser();
    http.expectOne('/api/User/3').flush(null);
    expect(auth.user()).toBeNull();
    expect(navigate).toHaveBeenCalledWith(['/'], { queryParams: { reason: 'password-changed' } });
    http.expectNone('/api/User');
  });
});
