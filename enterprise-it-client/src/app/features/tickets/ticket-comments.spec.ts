import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TicketComments } from './ticket-comments';

describe('TicketComments', () => {
  let fixture: ComponentFixture<TicketComments>;
  let http: HttpTestingController;
  const comment = { commentId: 1, ticketId: 42, user: 'Test Employee', comment: 'İlk mesaj\nİkinci satır', ticketTitle: 'Test', createdAt: '2026-10-26T09:00:00Z' };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [TicketComments], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    fixture = TestBed.createComponent(TicketComments);
    http = TestBed.inject(HttpTestingController);
    fixture.componentRef.setInput('ticketId', 42);
    fixture.detectChanges();
  });
  afterEach(() => http.verify());
  function load(comments = [comment]): void { http.expectOne('/api/TicketComment?ticketId=42').flush(comments); fixture.detectChanges(); }

  it('shows the author, date and plain-text message', () => {
    load();
    expect(fixture.nativeElement.textContent).toContain('Test Employee');
    expect(fixture.nativeElement.querySelector('time')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.comment-text').textContent).toContain('İkinci satır');
  });

  it('renders HTML as text rather than executable markup', () => {
    http.expectOne('/api/TicketComment?ticketId=42').flush([{ ...comment, comment: '<img src=x onerror=alert(1)>' }]); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.comment-text img')).toBeNull();
    expect(fixture.nativeElement.querySelector('.comment-text').textContent).toContain('<img');
  });

  it('posts only ticket ID and trimmed text, then reloads the history', () => {
    load([]);
    fixture.componentInstance.form.controls.comment.setValue(' Yeni mesaj ');
    fixture.componentInstance.sendComment();
    const request = http.expectOne('/api/TicketComment');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ ticketId: 42, comment: 'Yeni mesaj' });
    fixture.componentInstance.sendComment(); http.expectNone('/api/TicketComment');
    request.flush({ ...comment, comment: 'Yeni mesaj' });
    load([{ ...comment, comment: 'Yeni mesaj' }]);
    expect(fixture.nativeElement.textContent).toContain('Yorumunuz gönderildi.');
    expect(fixture.componentInstance.form.controls.comment.value).toBe('');
  });

  it('blocks whitespace-only and overlong comments', () => {
    load([]);
    for (const text of ['   ', 'x'.repeat(1001)]) {
      fixture.componentInstance.form.controls.comment.setValue(text);
      fixture.componentInstance.sendComment(); http.expectNone('/api/TicketComment');
      expect(fixture.componentInstance.form.invalid).toBe(true);
    }
  });

  it('keeps history readable for closed tickets without allowing a post', () => {
    load();
    fixture.componentRef.setInput('closed', true); fixture.detectChanges();
    fixture.componentInstance.form.controls.comment.setValue('Yeni mesaj');
    fixture.componentInstance.sendComment(); http.expectNone('/api/TicketComment');
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.nativeElement.querySelector('.comment-list')).not.toBeNull();
  });

  it('retains the draft after a network error', () => {
    load([]);
    fixture.componentInstance.form.controls.comment.setValue('Taslak mesaj'); fixture.componentInstance.sendComment();
    http.expectOne('/api/TicketComment').flush({}, { status: 503, statusText: 'Unavailable' });
    expect(fixture.componentInstance.form.controls.comment.value).toBe('Taslak mesaj');
    expect(fixture.componentInstance.posting()).toBe(false);
    expect(fixture.componentInstance.postError()).toContain('gönderilemedi');
  });

  it('notifies the parent when the ticket closed during posting', () => {
    load([]);
    const changed = vi.spyOn(fixture.componentInstance.ticketChanged, 'emit');
    fixture.componentInstance.form.controls.comment.setValue('Mesaj'); fixture.componentInstance.sendComment();
    http.expectOne('/api/TicketComment').flush('Kapalı ticket’a yeni yorum eklenemez.', { status: 409, statusText: 'Conflict' });
    expect(changed).toHaveBeenCalledWith('Kapalı ticket’a yeni yorum eklenemez.');
  });

  it('cancels stale history requests when the ticket changes', () => {
    const old = http.expectOne('/api/TicketComment?ticketId=42');
    fixture.componentRef.setInput('ticketId', 43); fixture.detectChanges();
    expect(old.cancelled).toBe(true);
    http.expectOne('/api/TicketComment?ticketId=43').flush([]);
    expect(fixture.componentInstance.comments().length).toBe(0);
  });

  it('retries a failed history load', () => {
    http.expectOne('/api/TicketComment?ticketId=42').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click(); load();
    expect(fixture.componentInstance.error()).toBe('');
  });
});
