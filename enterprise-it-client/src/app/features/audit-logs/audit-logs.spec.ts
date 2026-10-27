import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuditLogs } from './audit-logs';

describe('AuditLogs', () => {
  let fixture: ComponentFixture<AuditLogs>;
  let http: HttpTestingController;
  const log = { auditLogId: 1, action: 'Assign', entityName: 'Ticket', entityId: 42, userId: 7, user: 'Test Admin', createdAt: '2026-10-27T09:00:00Z' };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [AuditLogs], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    fixture = TestBed.createComponent(AuditLogs); http = TestBed.inject(HttpTestingController); fixture.detectChanges();
  });
  afterEach(() => http.verify());
  it('shows server metadata without editing controls', () => {
    http.expectOne('/api/AuditLog?page=1').flush({ items: [log], page: 1, hasMore: false }); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Test Admin');
    expect(fixture.nativeElement.textContent).toContain('Atama');
    expect(fixture.nativeElement.textContent).toContain('#42');
    expect(fixture.nativeElement.querySelector('input, form, textarea')).toBeNull();
  });
  it('navigates pages using the server hasMore flag', () => {
    http.expectOne('/api/AuditLog?page=1').flush({ items: [log], page: 1, hasMore: true }); fixture.detectChanges();
    fixture.nativeElement.querySelector('.pagination button:last-child').click();
    http.expectOne('/api/AuditLog?page=2').flush({ items: [{ ...log, auditLogId: 2 }], page: 2, hasMore: false }); fixture.detectChanges();
    expect(fixture.componentInstance.page()).toBe(2);
    expect(fixture.nativeElement.querySelector('.pagination button:last-child').disabled).toBe(true);
  });
  it('retries the failed destination page', () => {
    http.expectOne('/api/AuditLog?page=1').flush({ items: [log], page: 1, hasMore: true });
    fixture.componentInstance.load(2);
    http.expectOne('/api/AuditLog?page=2').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click();
    http.expectOne('/api/AuditLog?page=2').flush({ items: [], page: 2, hasMore: false });
    expect(fixture.componentInstance.error()).toBe('');
  });
  it('shows an empty state without fabricating historical records', () => {
    http.expectOne('/api/AuditLog?page=1').flush({ items: [], page: 1, hasMore: false }); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('audit kaydı bulunmuyor');
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });
  it('refreshes the latest records from page one', () => {
    http.expectOne('/api/AuditLog?page=1').flush({ items: [log], page: 1, hasMore: true });
    fixture.componentInstance.load(2); http.expectOne('/api/AuditLog?page=2').flush({ items: [], page: 2, hasMore: false }); fixture.detectChanges();
    fixture.nativeElement.querySelector('header button').click();
    http.expectOne('/api/AuditLog?page=1').flush({ items: [log], page: 1, hasMore: true });
    expect(fixture.componentInstance.page()).toBe(1);
  });
});
