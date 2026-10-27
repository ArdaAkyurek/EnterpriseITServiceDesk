import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuditLog } from '../../models/audit-log';
import { AuditLogService } from '../../services/audit-log';

@Component({
  selector: 'app-audit-logs', standalone: true, imports: [DatePipe],
  templateUrl: './audit-logs.html', styleUrls: ['../../assets/assets.css', './audit-logs.css']
})
export class AuditLogs implements OnInit {
  private readonly service = inject(AuditLogService);
  private readonly destroyRef = inject(DestroyRef);
  private requestedPage = 1;
  readonly logs = signal<AuditLog[]>([]);
  readonly page = signal(1);
  readonly hasMore = signal(false);
  readonly loading = signal(false);
  readonly error = signal('');
  private readonly actions: Record<string, string> = { Create: 'Oluşturma', Update: 'Güncelleme', Delete: 'Silme',
    Assign: 'Atama', Claim: 'Üzerine alma', Start: 'İşe başlama', Close: 'Kapatma', PhoneUpdated: 'Telefon güncelleme', PasswordChanged: 'Parola değişikliği' };
  private readonly entities: Record<string, string> = { User: 'Kullanıcı', Department: 'Departman', Asset: 'Demirbaş',
    Ticket: 'Ticket', TicketComment: 'Ticket yorumu', TicketAttachment: 'Dosya eki', AssetAssignment: 'Zimmet', AssetMaintenance: 'Bakım',
    Role: 'Rol', AssetType: 'Demirbaş türü', TicketCategory: 'Ticket kategorisi', TicketPriority: 'Ticket önceliği', TicketStatus: 'Ticket durumu' };

  ngOnInit(): void { this.load(); }
  actionLabel(action: string): string { return Object.hasOwn(this.actions, action) ? this.actions[action] : action; }
  entityLabel(entity: string): string { return Object.hasOwn(this.entities, entity) ? this.entities[entity] : entity; }

  load(page = this.requestedPage): void {
    if (this.loading() || page < 1) return;
    this.requestedPage = page;
    this.loading.set(true); this.error.set('');
    this.service.getLogs(page).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => { this.logs.set(result.items); this.page.set(result.page); this.hasMore.set(result.hasMore); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Audit kayıtları yüklenemedi. Tekrar deneyin.'); }
    });
  }
}
