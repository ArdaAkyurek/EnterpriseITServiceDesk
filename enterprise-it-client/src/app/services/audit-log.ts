import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuditLogPage } from '../models/audit-log';

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly http = inject(HttpClient);
  getLogs(page: number) { return this.http.get<AuditLogPage>('/api/AuditLog', { params: { page } }); }
}
