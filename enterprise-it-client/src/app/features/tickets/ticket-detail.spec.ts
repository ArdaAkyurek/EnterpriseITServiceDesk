import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { BehaviorSubject, of } from 'rxjs';
import { TicketService } from '../../services/ticket';
import { TicketAttachmentService } from '../../services/ticket-attachment';
import { TicketDetail } from './ticket-detail';
import { AuthService } from '../../services/auth';
import { UserRole } from '../../models/auth';

describe('TicketDetail', () => {
  let fixture: ComponentFixture<TicketDetail>;
  let http: HttpTestingController;
  let params: BehaviorSubject<ReturnType<typeof convertToParamMap>>;
  const ticket = {
    ticketId: 42, title: 'Ekran açılmıyor', description: 'Güç ışığı yanıyor.\nGörüntü yok.',
    createdBy: 'Test Employee', assignedTo: null, assignedToUserId: null, updatedAt: null,
    category: 'Hardware', priority: 'High', status: 'Open', asset: null,
    createdAt: '2026-10-26T09:00:00Z', closedAt: null
  };

  beforeEach(async () => {
    params = new BehaviorSubject(convertToParamMap({ id: '42' }));
    await TestBed.configureTestingModule({
      imports: [TicketDetail], providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]),
        { provide: ActivatedRoute, useValue: { paramMap: params.asObservable() } }]
    }).compileComponents();
    fixture = TestBed.createComponent(TicketDetail);
    http = TestBed.inject(HttpTestingController);
    vi.spyOn(TestBed.inject(TicketService), 'getComments').mockReturnValue(of([]));
    vi.spyOn(TestBed.inject(TicketAttachmentService), 'getAttachments').mockReturnValue(of([]));
    fixture.detectChanges();
  });
  afterEach(() => { http.verify(); TestBed.inject(AuthService).clearSession(); });

  function role(role: UserRole): void {
    TestBed.inject(AuthService).user.set({ userId: 5, firstName: 'Test', lastName: role, email: 'test@example.com', role });
  }
  function loadAdmin(): void {
    role('Admin');
    http.expectOne('/api/Ticket/42').flush(ticket);
    http.expectOne('/api/Ticket/assignment-options').flush([{ userId: 7, fullName: 'Active Technician' }]);
    fixture.detectChanges();
  }

  it('shows details to an employee without assignment controls', () => {
    role('Employee');
    http.expectOne('/api/Ticket/42').flush(ticket);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Ekran açılmıyor');
    expect(fixture.nativeElement.querySelector('.description').textContent).toContain('Görüntü yok.');
    expect(fixture.nativeElement.querySelector('#assignment-title')).toBeNull();
    expect(fixture.nativeElement.querySelector('#claim-title')).toBeNull();
    http.expectNone('/api/Ticket/assignment-options');
  });

  it('handles a missing or inaccessible ticket', () => {
    http.expectOne('/api/Ticket/42').flush({}, { status: 404, statusText: 'Not Found' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('erişiminiz yok');
    expect(fixture.nativeElement.querySelector('article')).toBeNull();
  });

  it('assigns as Admin using the previously loaded version', () => {
    loadAdmin();
    fixture.componentInstance.assignee.setValue(7);
    fixture.componentInstance.assign();
    const request = http.expectOne('/api/Ticket/42/assignment');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ assignedToUserId: 7, expectedAssignedToUserId: null, expectedUpdatedAt: null });
    fixture.componentInstance.assign();
    http.expectNone('/api/Ticket/42/assignment');
    request.flush({ ...ticket, assignedTo: 'Active Technician', assignedToUserId: 7, updatedAt: '2026-10-26T10:00:00Z' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Ticket teknisyene atandı.');
    expect(fixture.nativeElement.textContent).toContain('Active Technician');
  });

  it('claims as Technician without a client-supplied assignee', () => {
    role('Technician');
    http.expectOne('/api/Ticket/42').flush(ticket);
    fixture.detectChanges();
    fixture.nativeElement.querySelector('#claim-title').parentElement.querySelector('button').click();
    const request = http.expectOne('/api/Ticket/42/claim');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ expectedUpdatedAt: null });
    request.flush({ ...ticket, assignedTo: 'Test Technician', assignedToUserId: 5 });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#claim-title')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Ticket üzerinize alındı.');
  });

  it('reloads after a conflicting claim and preserves the conflict message', () => {
    role('Technician');
    http.expectOne('/api/Ticket/42').flush(ticket);
    fixture.componentInstance.claim();
    http.expectOne('/api/Ticket/42/claim').flush('Ticket başka bir işlemle değiştirildi.', { status: 409, statusText: 'Conflict' });
    http.expectOne('/api/Ticket/42').flush({ ...ticket, assignedTo: 'Other Technician', assignedToUserId: 7 });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Ticket başka bir işlemle değiştirildi.');
    expect(fixture.nativeElement.textContent).toContain('Other Technician');
    expect(fixture.nativeElement.querySelector('#claim-title')).toBeNull();
  });

  it('does not offer assignment or claim for closed tickets', () => {
    role('Admin');
    http.expectOne('/api/Ticket/42').flush({ ...ticket, status: 'Closed', closedAt: '2026-10-26T10:00:00Z' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#assignment-title')).toBeNull();
    http.expectNone('/api/Ticket/assignment-options');
    fixture.componentInstance.assign();
    fixture.componentInstance.claim();
    http.expectNone('/api/Ticket/42/assignment');
    http.expectNone('/api/Ticket/42/claim');
  });

  it('shows when no active technician is available', () => {
    role('Admin');
    http.expectOne('/api/Ticket/42').flush(ticket);
    http.expectOne('/api/Ticket/assignment-options').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Aktif teknisyen bulunmuyor.');
  });

  it('cancels an old detail request when the route changes', () => {
    const old = http.expectOne('/api/Ticket/42');
    params.next(convertToParamMap({ id: '43' }));
    expect(old.cancelled).toBe(true);
    http.expectOne('/api/Ticket/43').flush({ ...ticket, ticketId: 43 });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h2').textContent).toContain('#43');
  });

  it('stops loading for an invalid route ID without sending a request', () => {
    const old = http.expectOne('/api/Ticket/42');
    params.next(convertToParamMap({ id: 'invalid' }));
    expect(old.cancelled).toBe(true);
    http.expectNone('/api/Ticket/NaN');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('Geçersiz ticket numarası.');
    expect(fixture.componentInstance.loading()).toBe(false);
  });

  it('starts its assigned ticket as Technician with the loaded version', () => {
    role('Technician');
    http.expectOne('/api/Ticket/42').flush({ ...ticket, assignedToUserId: 5, assignedTo: 'Test Technician' });
    fixture.componentInstance.changeStatus('In Progress');
    const request = http.expectOne('/api/Ticket/42/status');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ status: 'In Progress', expectedStatus: 'Open', expectedAssignedToUserId: 5, expectedUpdatedAt: null });
    fixture.componentInstance.changeStatus('In Progress');
    http.expectNone('/api/Ticket/42/status');
    request.flush({ ...ticket, assignedToUserId: 5, status: 'In Progress' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Ticket üzerinde çalışmaya başlandı.');
  });

  it('requires confirmation to close and shows the returned closing time', () => {
    role('Technician');
    http.expectOne('/api/Ticket/42').flush({ ...ticket, assignedToUserId: 5, status: 'In Progress' });
    fixture.componentInstance.changeStatus('Closed');
    http.expectNone('/api/Ticket/42/status');
    fixture.detectChanges();
    const section = fixture.nativeElement.querySelector('#ticket-status-title').parentElement;
    section.querySelector('button').click();
    fixture.detectChanges();
    fixture.nativeElement.querySelector('.close-confirmation .primary').click();
    http.expectOne('/api/Ticket/42/status').flush({ ...ticket, assignedToUserId: 5, status: 'Closed', closedAt: '2026-10-26T10:00:00Z' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Ticket kapatıldı.');
    expect(fixture.nativeElement.textContent).toContain('Kapanış');
    expect(fixture.nativeElement.querySelector('#ticket-status-title')).toBeNull();
  });

  it.each(['Employee', 'Technician'] as UserRole[])('prevents %s from changing another assignee’s ticket', userRole => {
    role(userRole);
    http.expectOne('/api/Ticket/42').flush({ ...ticket, assignedToUserId: 7 });
    fixture.componentInstance.changeStatus('In Progress');
    http.expectNone('/api/Ticket/42/status');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#ticket-status-title')).toBeNull();
  });

  it('requires assignment before Admin can start a ticket', () => {
    loadAdmin();
    fixture.componentInstance.changeStatus('In Progress');
    http.expectNone('/api/Ticket/42/status');
    expect(fixture.nativeElement.querySelector('#ticket-status-title').parentElement.textContent).toContain('aktif bir teknisyene atayın');
  });

  it('refreshes after a conflicting status change', () => {
    role('Technician');
    http.expectOne('/api/Ticket/42').flush({ ...ticket, assignedToUserId: 5 });
    fixture.componentInstance.changeStatus('In Progress');
    http.expectOne('/api/Ticket/42/status').flush('Ticket başka bir işlemle değiştirildi.', { status: 409, statusText: 'Conflict' });
    http.expectOne('/api/Ticket/42').flush({ ...ticket, assignedToUserId: 7 });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Ticket başka bir işlemle değiştirildi.');
    expect(fixture.nativeElement.querySelector('#ticket-status-title')).toBeNull();
  });
});
