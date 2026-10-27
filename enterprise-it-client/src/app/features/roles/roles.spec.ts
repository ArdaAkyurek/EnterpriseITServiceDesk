import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Roles } from './roles';

describe('Roles', () => {
  let fixture: ComponentFixture<Roles>;
  let http: HttpTestingController;
  const roles = [{ roleId: 1, roleName: 'Admin', description: null }, { roleId: 2, roleName: 'Technician' }, { roleId: 3, roleName: 'Employee' }];
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Roles], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] }).compileComponents();
    fixture = TestBed.createComponent(Roles); http = TestBed.inject(HttpTestingController); fixture.detectChanges();
  });
  afterEach(() => http.verify());

  it('shows API roles, descriptions and a readonly comparison', () => {
    const request = http.expectOne('/api/Role');
    expect(request.request.method).toBe('GET'); request.flush(roles); fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.role-card').length).toBe(3);
    expect(fixture.nativeElement.textContent).toContain('Yönetici');
    expect(fixture.nativeElement.textContent).toContain('Kendi talepleri');
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(13);
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.nativeElement.querySelector('a').getAttribute('href')).toBe('/dashboard/users');
  });
  it('accurately distinguishes assigning from claiming a ticket', () => {
    http.expectOne('/api/Role').flush(roles); fixture.detectChanges();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(Array.from(rows[2].querySelectorAll('td') as NodeListOf<HTMLTableCellElement>).map(cell => cell.textContent?.trim())).toEqual(['Var', 'Yok', 'Yok']);
    expect(Array.from(rows[3].querySelectorAll('td') as NodeListOf<HTMLTableCellElement>).map(cell => cell.textContent?.trim())).toEqual(['Yok', 'Var', 'Yok']);
  });
  it('shows unknown roles without implying they have permissions', () => {
    http.expectOne('/api/Role').flush([{ roleId: 99, roleName: 'Custom', description: 'Imported role' }]); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Desteklenmeyen rol');
    expect(fixture.nativeElement.textContent).toContain('Imported role');
    expect(fixture.nativeElement.querySelector('tbody td').textContent).toContain('Desteklenmiyor');
  });
  it('shows an empty state', () => {
    http.expectOne('/api/Role').flush([]); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Kayıtlı rol bulunmuyor.');
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });
  it('allows retry after an API error', () => {
    http.expectOne('/api/Role').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click();
    http.expectOne('/api/Role').flush(roles); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
    expect(fixture.nativeElement.querySelectorAll('.role-card').length).toBe(3);
  });
});
