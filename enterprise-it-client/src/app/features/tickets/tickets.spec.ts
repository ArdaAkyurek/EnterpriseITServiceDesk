import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Tickets } from './tickets';
import { AuthService } from '../../services/auth';

describe('Tickets', () => {
  let fixture: ComponentFixture<Tickets>;
  let http: HttpTestingController;
  const options = {
    categories: [{ ticketCategoryId: 1, ticketCategoryName: 'Hardware' }],
    priorities: [{ ticketPriorityId: 2, priorityName: 'Medium' }],
    assets: []
  };
  const ticket = {
    ticketId: 42, title: 'Ekran açılmıyor', description: 'Güç ışığı yanıyor.',
    createdBy: 'Test User', assignedTo: null, category: 'Hardware', priority: 'Medium',
    status: 'Open', asset: null, createdAt: '2026-10-26T09:00:00Z', closedAt: null
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Tickets],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    }).compileComponents();
    fixture = TestBed.createComponent(Tickets);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });
  afterEach(() => { http.verify(); TestBed.inject(AuthService).clearSession(); });

  function prepareForm(): void {
    http.expectOne('/api/Ticket').flush([]);
    fixture.componentInstance.openForm();
    http.expectOne('/api/Ticket/form-options').flush(options);
    fixture.componentInstance.form.setValue({
      title: ' Ekran açılmıyor ', description: ' Güç ışığı yanıyor. ', categoryId: 1, priorityId: 2, assetId: null
    });
    fixture.detectChanges();
  }

  it('renders the returned list with unassigned and optional asset labels', () => {
    expect(fixture.nativeElement.textContent).toContain('yükleniyor');
    http.expectOne('/api/Ticket').flush([ticket]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Ekran açılmıyor');
    expect(fixture.nativeElement.textContent).toContain('Henüz atanmadı');
    expect(fixture.nativeElement.textContent).toContain('Belirtilmedi');
  });

  it('shows an empty list and retries failed loading', () => {
    http.expectOne('/api/Ticket').flush('Unavailable', { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click();
    http.expectOne('/api/Ticket').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Henüz ticket bulunmuyor.');
  });

  it('creates a ticket without a client-supplied owner or status and refreshes the list', () => {
    prepareForm();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    const request = http.expectOne('/api/Ticket');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      title: 'Ekran açılmıyor', description: 'Güç ışığı yanıyor.', categoryId: 1, priorityId: 2, assetId: null
    });
    fixture.componentInstance.saveTicket();
    http.expectNone('/api/Ticket');
    request.flush(ticket);
    http.expectOne('/api/Ticket').flush([ticket]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('#42 numaralı ticket oluşturuldu.');
  });

  it('rejects whitespace-only fields before sending a request', () => {
    prepareForm();
    fixture.componentInstance.form.controls.title.setValue('   ');
    fixture.componentInstance.saveTicket();
    http.expectNone('/api/Ticket');
    expect(fixture.componentInstance.form.controls.title.invalid).toBe(true);
  });

  it('retains input and displays server validation errors', () => {
    prepareForm();
    fixture.componentInstance.saveTicket();
    http.expectOne('/api/Ticket').flush('Geçersiz Ticket Category.', { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form [role="alert"]').textContent).toContain('Geçersiz Ticket Category.');
    expect(fixture.componentInstance.form.controls.title.value).toBe(' Ekran açılmıyor ');
    expect(fixture.componentInstance.saving()).toBe(false);
  });

  it('blocks saving until options load successfully and allows retry', () => {
    http.expectOne('/api/Ticket').flush([]);
    fixture.componentInstance.openForm();
    http.expectOne('/api/Ticket/form-options').flush({}, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(true);
    fixture.nativeElement.querySelector('form [role="alert"] button').click();
    http.expectOne('/api/Ticket/form-options').flush(options);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button[type="submit"]').disabled).toBe(false);
  });

  it('allows an employee to create a ticket even without assigned assets', () => {
    TestBed.inject(AuthService).user.set({ userId: 3, firstName: 'Test', lastName: 'Employee', email: 'employee@example.com', role: 'Employee' });
    prepareForm();
    expect(fixture.nativeElement.textContent).toContain('Yalnızca üzerinize zimmetli demirbaşlar listelenir.');
    expect(fixture.componentInstance.form.valid).toBe(true);
    fixture.componentInstance.saveTicket();
    http.expectOne('/api/Ticket').flush(ticket);
    http.expectOne('/api/Ticket?view=created').flush([ticket]);
  });

  function technician(): void {
    TestBed.inject(AuthService).user.set({ userId: 7, firstName: 'Test', lastName: 'Technician', email: 'tech@example.com', role: 'Technician' });
  }

  it('loads assigned tickets with a server filter and a matching empty message', () => {
    technician();
    http.expectOne('/api/Ticket').flush([]);
    fixture.componentInstance.changeView('assigned');
    http.expectOne('/api/Ticket?view=assigned').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Size atanmış ticket bulunmuyor.');
    expect(fixture.nativeElement.querySelector('[aria-pressed="true"]').textContent).toContain('Bana atananlar');
  });

  it('cancels old list requests when filters change quickly', () => {
    technician();
    const initial = http.expectOne('/api/Ticket');
    fixture.componentInstance.changeView('assigned');
    expect(initial.cancelled).toBe(true);
    const assigned = http.expectOne('/api/Ticket?view=assigned');
    fixture.componentInstance.changeView('unassigned');
    expect(assigned.cancelled).toBe(true);
    http.expectOne('/api/Ticket?view=unassigned').flush([ticket]);
    fixture.detectChanges();
    expect(fixture.componentInstance.currentView()).toBe('unassigned');
    expect(fixture.nativeElement.querySelector('.count').textContent).toContain('Atanmamışlar');
  });

  it('retries the currently selected worklist after an error', () => {
    technician();
    http.expectOne('/api/Ticket').flush([]);
    fixture.componentInstance.changeView('unassigned');
    http.expectOne('/api/Ticket?view=unassigned').flush({}, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click();
    http.expectOne('/api/Ticket?view=unassigned').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Atanmamış ticket bulunmuyor.');
  });

  it('shows only own requests for employees and blocks other worklist selections', () => {
    const initial = http.expectOne('/api/Ticket');
    TestBed.inject(AuthService).user.set({ userId: 3, firstName: 'Test', lastName: 'Employee', email: 'employee@example.com', role: 'Employee' });
    fixture.componentInstance.loadTickets();
    expect(initial.cancelled).toBe(true);
    http.expectOne('/api/Ticket?view=created').flush([]);
    fixture.componentInstance.changeView('all');
    http.expectNone('/api/Ticket');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.worklists')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Taleplerim');
    expect(fixture.nativeElement.textContent).toContain('Henüz oluşturduğunuz bir talep bulunmuyor.');
  });

  it('preserves the selected list after a ticket is created', () => {
    technician();
    prepareForm();
    fixture.componentInstance.changeView('assigned');
    http.expectOne('/api/Ticket?view=assigned').flush([]);
    fixture.componentInstance.saveTicket();
    http.expectOne('/api/Ticket').flush(ticket);
    http.expectOne('/api/Ticket?view=assigned').flush([]);
    expect(fixture.componentInstance.currentView()).toBe('assigned');
  });
});
