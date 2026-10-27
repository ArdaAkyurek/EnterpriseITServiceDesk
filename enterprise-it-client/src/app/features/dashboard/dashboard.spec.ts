import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Dashboard } from './dashboard';
import { AuthService } from '../../services/auth';
import { DashboardSummary } from '../../models/dashboard';

describe('Dashboard', () => {
  let fixture: ComponentFixture<Dashboard>;
  let http: HttpTestingController;
  let auth: AuthService;
  const summary: DashboardSummary = {
    scope: 'all', total: 10, open: 4, inProgress: 3, closed: 2, other: 1, unassignedActiveCount: 3,
    activeByPriority: [{ priority: 'High', count: 8 }],
    recentTickets: [{ ticketId: 42, title: 'Ekran açılmıyor', status: 'Open', priority: 'High', assignedTo: null, createdAt: '2026-10-27T09:00:00Z' }],
    generatedAt: '2026-10-27T09:30:00Z'
  };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Dashboard], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])] }).compileComponents();
    fixture = TestBed.createComponent(Dashboard); http = TestBed.inject(HttpTestingController); auth = TestBed.inject(AuthService);
    auth.user.set({ userId: 7, firstName: 'Test', lastName: 'Admin', email: 'test@example.invalid', role: 'Admin' }); fixture.detectChanges();
  });
  afterEach(() => { http.verify(); fixture.destroy(); auth.clearSession(); });
  it('shows loading without fabricated zero counts', () => {
    expect(fixture.nativeElement.querySelector('[role="status"]').textContent).toContain('hazırlanıyor');
    expect(fixture.nativeElement.querySelector('.metrics')).toBeNull(); http.expectOne('/api/Dashboard').flush(summary);
  });
  it('shows server totals, status distribution and recent ticket links', () => {
    http.expectOne('/api/Dashboard').flush(summary); fixture.detectChanges();
    const values = [...fixture.nativeElement.querySelectorAll('.metric strong')].map((element: any) => element.textContent.trim());
    expect(values).toEqual(['10', '4', '3', '2']); expect(fixture.nativeElement.textContent).toContain('Tüm ticket’lar');
    expect(fixture.nativeElement.querySelector('tbody a').getAttribute('href')).toBe('/dashboard/tickets/42');
    expect(fixture.nativeElement.textContent).toContain('Henüz atanmadı'); expect(fixture.componentInstance.activeCount()).toBe(8);
    expect(fixture.nativeElement.querySelector('.legend').textContent).toContain('Diğer');
  });
  it('limits Employee navigation and shows the personal server scope', () => {
    auth.user.update(user => ({ ...user!, role: 'Employee' }));
    http.expectOne('/api/Dashboard').flush({ ...summary, scope: 'created', unassignedActiveCount: null }); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Oluşturduğum talepler'); expect(fixture.nativeElement.querySelector('.queue')).toBeNull();
    expect(fixture.nativeElement.querySelector('a[href="/dashboard/assets"]')).toBeNull(); expect(fixture.nativeElement.querySelector('a[href="/dashboard/users"]')).toBeNull();
    expect(fixture.nativeElement.querySelector('.primary-link').textContent).toContain('Taleplerime git');
  });
  it('shows Technician scope and queue without Admin links', () => {
    auth.user.update(user => ({ ...user!, role: 'Technician' }));
    http.expectOne('/api/Dashboard').flush({ ...summary, scope: 'assigned' }); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Bana atanan ticket’lar'); expect(fixture.nativeElement.querySelector('.queue').textContent).toContain('3 atanmamış');
    expect(fixture.nativeElement.querySelector('a[href="/dashboard/users"]')).toBeNull();
  });
  it('shows useful empty states without NaN bars', () => {
    http.expectOne('/api/Dashboard').flush({ ...summary, total: 0, open: 0, inProgress: 0, closed: 0, other: 0, activeByPriority: [], recentTickets: [] }); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.stack')).toBeNull(); expect(fixture.nativeElement.textContent).toContain('Aktif ticket bulunmuyor');
    expect(fixture.nativeElement.textContent).toContain('Henüz gösterilecek ticket yok'); expect(fixture.nativeElement.innerHTML).not.toContain('NaN');
  });
  it('offers a retry after failure without presenting missing data as zero', () => {
    http.expectOne('/api/Dashboard').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.metrics')).toBeNull(); fixture.nativeElement.querySelector('[role="alert"] button').click();
    http.expectOne('/api/Dashboard').flush(summary); fixture.detectChanges(); expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });
  it('retains previous data with a stale-data warning if refresh fails', () => {
    http.expectOne('/api/Dashboard').flush(summary); fixture.componentInstance.load();
    http.expectOne('/api/Dashboard').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    expect(fixture.componentInstance.summary()?.total).toBe(10); expect(fixture.nativeElement.textContent).toContain('son başarılı yüklemeye aittir');
  });
  it('refreshes once while preventing duplicate requests', () => {
    http.expectOne('/api/Dashboard').flush(summary); fixture.componentInstance.load(); fixture.componentInstance.load();
    http.expectOne('/api/Dashboard').flush({ ...summary, total: 11 }); expect(fixture.componentInstance.summary()?.total).toBe(11);
  });
  it('cancels the request when leaving the page', () => {
    const request = http.expectOne('/api/Dashboard'); fixture.destroy(); expect(request.cancelled).toBe(true);
  });
  it('renders ticket titles as text and translates status labels', () => {
    http.expectOne('/api/Dashboard').flush({ ...summary, recentTickets: [{ ...summary.recentTickets[0], title: '<img src=x onerror=alert(1)>', status: 'In Progress' }] }); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img')).toBeNull(); expect(fixture.nativeElement.querySelector('tbody').textContent).toContain('İşlemde');
    expect(fixture.componentInstance.statusLabel('Custom')).toBe('Custom');
  });
});
