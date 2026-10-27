import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TicketAttachment } from '../models/ticket-attachment';

@Injectable({ providedIn: 'root' })
export class TicketAttachmentService {
  private readonly http = inject(HttpClient);
  getAttachments(ticketId: number) {
    return this.http.get<TicketAttachment[]>('/api/TicketAttachment', { params: { ticketId } });
  }
  upload(ticketId: number, file: File) {
    const body = new FormData();
    body.append('ticketId', String(ticketId)); body.append('file', file);
    return this.http.post<TicketAttachment>('/api/TicketAttachment', body);
  }
  download(id: number) {
    return this.http.get(`/api/TicketAttachment/${id}/download`, { responseType: 'blob' });
  }
}
