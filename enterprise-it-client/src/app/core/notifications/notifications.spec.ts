import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Notifications } from './notifications';

describe('Notifications', () => {
  let fixture: ComponentFixture<Notifications>;
  let http: HttpTestingController;
  const item = { notificationId: 5, title: 'Ticket atandı', message: '#42 size atandı.', isRead: false, createdAt: '2026-10-27T09:00:00Z' };
  const page = { items: [item], page: 1, hasMore: false, unreadCount: 1 };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Notifications], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    fixture = TestBed.createComponent(Notifications); http = TestBed.inject(HttpTestingController); fixture.detectChanges();
  });
  afterEach(() => { http.verify(); fixture.destroy(); });
  function open() {
    http.expectOne('/api/Notification?page=1').flush(page);
    fixture.componentInstance.toggle();
    http.expectOne('/api/Notification?page=1').flush(page); fixture.detectChanges();
  }
  it('loads the unread badge and refreshes the panel when opened', () => {
    open(); expect(fixture.nativeElement.querySelector('.badge').textContent).toContain('1');
    expect(fixture.nativeElement.textContent).toContain('#42 size atandı.');
    expect(fixture.nativeElement.querySelector('.bell').getAttribute('aria-expanded')).toBe('true');
  });
  it('marks a notification read once and refreshes the server count', () => {
    open(); fixture.componentInstance.markRead(item); fixture.componentInstance.markRead(item);
    const request = http.expectOne('/api/Notification/5/read'); expect(request.request.method).toBe('PUT'); expect(request.request.body).toEqual({});
    request.flush(null);
    http.expectOne('/api/Notification?page=1').flush({ ...page, items: [{ ...item, isRead: true }], unreadCount: 0 }); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.badge')).toBeNull(); expect(fixture.nativeElement.textContent).toContain('Okundu');
    fixture.componentInstance.markRead({ ...item, isRead: true }); http.expectNone('/api/Notification/5/read');
  });
  it('preserves unread data after a failed mark and permits retry', () => {
    open(); fixture.componentInstance.markRead(item);
    http.expectOne('/api/Notification/5/read').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    expect(fixture.componentInstance.items()[0].isRead).toBe(false); expect(fixture.componentInstance.unreadCount()).toBe(1);
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('işaretlenemedi');
    fixture.componentInstance.markRead(item); http.expectOne('/api/Notification/5/read').flush(null);
    http.expectOne('/api/Notification?page=1').flush({ ...page, unreadCount: 0 });
  });
  it('paginates using hasMore and retries the failed destination', () => {
    http.expectOne('/api/Notification?page=1').flush({ ...page, hasMore: true });
    fixture.componentInstance.load(2);
    http.expectOne('/api/Notification?page=2').flush({}, { status: 503, statusText: 'Unavailable' });
    expect(fixture.componentInstance.page()).toBe(1); fixture.componentInstance.load();
    http.expectOne('/api/Notification?page=2').flush({ ...page, page: 2 });
    expect(fixture.componentInstance.page()).toBe(2); expect(fixture.componentInstance.hasMore()).toBe(false);
  });
  it('shows an empty state', () => {
    http.expectOne('/api/Notification?page=1').flush({ ...page, items: [], unreadCount: 0 });
    fixture.componentInstance.toggle(); http.expectOne('/api/Notification?page=1').flush({ ...page, items: [], unreadCount: 0 }); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Henüz bildiriminiz yok.');
  });
  it('renders notification text without interpreting HTML', () => {
    http.expectOne('/api/Notification?page=1').flush(page); fixture.componentInstance.toggle();
    http.expectOne('/api/Notification?page=1').flush({ ...page, items: [{ ...item, message: '<img src=x onerror=alert(1)>' }] }); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img')).toBeNull(); expect(fixture.nativeElement.textContent).toContain('<img');
  });
  it('cancels a pending request on destroy', () => {
    const request = http.expectOne('/api/Notification?page=1'); fixture.destroy(); expect(request.cancelled).toBe(true);
  });
});
