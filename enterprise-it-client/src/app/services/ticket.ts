import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateTicket, Ticket, TicketAssignee, TicketComment, TicketFormOptions, TicketListView, TicketTargetStatus } from '../models/ticket';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Ticket';

  getTickets(view: TicketListView = 'all') {
    return this.http.get<Ticket[]>(this.apiUrl, view === 'all' ? {} : { params: { view } });
  }
  getFormOptions() { return this.http.get<TicketFormOptions>(`${this.apiUrl}/form-options`); }
  createTicket(ticket: CreateTicket) { return this.http.post<Ticket>(this.apiUrl, ticket); }
  getTicket(id: number) { return this.http.get<Ticket>(`${this.apiUrl}/${id}`); }
  getComments(ticketId: number) { return this.http.get<TicketComment[]>('/api/TicketComment', { params: { ticketId } }); }
  addComment(ticketId: number, comment: string) { return this.http.post<TicketComment>('/api/TicketComment', { ticketId, comment }); }
  getAssignees() { return this.http.get<TicketAssignee[]>(`${this.apiUrl}/assignment-options`); }
  assignTicket(ticket: Ticket, userId: number) {
    return this.http.put<Ticket>(`${this.apiUrl}/${ticket.ticketId}/assignment`, {
      assignedToUserId: userId, expectedAssignedToUserId: ticket.assignedToUserId ?? null,
      expectedUpdatedAt: ticket.updatedAt ?? null
    });
  }
  claimTicket(ticket: Ticket) {
    return this.http.post<Ticket>(`${this.apiUrl}/${ticket.ticketId}/claim`, { expectedUpdatedAt: ticket.updatedAt ?? null });
  }
  changeStatus(ticket: Ticket, status: TicketTargetStatus) {
    return this.http.put<Ticket>(`${this.apiUrl}/${ticket.ticketId}/status`, {
      status, expectedStatus: ticket.status, expectedAssignedToUserId: ticket.assignedToUserId ?? null,
      expectedUpdatedAt: ticket.updatedAt ?? null
    });
  }
}
