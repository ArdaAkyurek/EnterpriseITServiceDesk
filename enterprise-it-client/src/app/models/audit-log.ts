export interface AuditLog {
  auditLogId: number;
  action: string;
  entityName: string;
  entityId: number;
  userId: number;
  user: string;
  createdAt: string;
}

export interface AuditLogPage { items: AuditLog[]; page: number; hasMore: boolean; }
