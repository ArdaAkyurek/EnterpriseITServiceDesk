import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, catchError, distinctUntilChanged, map, of, startWith, switchMap, tap } from 'rxjs';
import { Ticket, TicketAssignee, TicketTargetStatus } from '../../models/ticket';
import { TicketService } from '../../services/ticket';
import { AuthService } from '../../services/auth';
import { TicketComments } from './ticket-comments';
import { TicketAttachments } from './ticket-attachments';

@Component({
  selector: 'app-ticket-detail', standalone: true,
  imports: [DatePipe, RouterLink, ReactiveFormsModule, TicketComments, TicketAttachments],
  templateUrl: './ticket-detail.html',
  styleUrls: ['../../assets/assets.css', './ticket-detail.css']
})
export class TicketDetail implements OnInit {
  private readonly service = inject(TicketService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refresh = new Subject<void>();
  readonly auth = inject(AuthService);
  readonly ticket = signal<Ticket | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly saving = signal(false);
  readonly actionError = signal('');
  readonly success = signal('');
  readonly closeRequested = signal(false);
  readonly technicians = signal<TicketAssignee[]>([]);
  readonly techniciansLoading = signal(false);
  readonly techniciansError = signal('');
  readonly assignee = new FormControl(0, { nonNullable: true, validators: [Validators.min(1)] });
  readonly closed = computed(() => !!this.ticket()?.closedAt || this.ticket()?.status === 'Closed');
  readonly canClaim = computed(() => this.auth.hasRole('Technician') && !!this.ticket() &&
    this.ticket()!.assignedToUserId == null && !this.closed());
  readonly canChangeStatus = computed(() => !!this.ticket() && !this.closed() &&
    (this.auth.hasRole('Admin') || this.auth.hasRole('Technician') && this.ticket()!.assignedToUserId === this.auth.user()?.userId));

  ngOnInit(): void {
    this.route.paramMap.pipe(
      map(params => Number(params.get('id'))), distinctUntilChanged(),
      tap(() => { this.saving.set(false); this.actionError.set(''); this.success.set(''); this.closeRequested.set(false); }),
      switchMap(id => this.refresh.pipe(startWith(undefined), switchMap(() => {
        this.ticket.set(null);
        this.loading.set(false);
        this.error.set('');
        this.technicians.set([]);
        this.techniciansLoading.set(false);
        this.techniciansError.set('');
        if (!Number.isSafeInteger(id) || id < 1) {
          this.error.set('Geçersiz ticket numarası.'); return of(null);
        }
        this.loading.set(true);
        return this.service.getTicket(id).pipe(
          tap(ticket => {
            this.ticket.set(ticket); this.loading.set(false);
            if (this.auth.hasRole('Admin') && !this.closed()) this.loadTechnicians();
          }),
          catchError((response: HttpErrorResponse) => {
            this.loading.set(false);
            this.error.set(response.status === 404 ? 'Ticket bulunamadı veya bu ticket’a erişiminiz yok.'
              : 'Ticket yüklenemedi. Tekrar deneyin.');
            return of(null);
          })
        );
      }))), takeUntilDestroyed(this.destroyRef)
    ).subscribe();
  }

  reload(): void {
    if (this.saving()) return;
    this.actionError.set(''); this.success.set(''); this.closeRequested.set(false); this.refresh.next();
  }

  loadTechnicians(): void {
    const ticket = this.ticket();
    if (!ticket || !this.auth.hasRole('Admin') || this.closed() || this.techniciansLoading()) return;
    this.techniciansLoading.set(true); this.techniciansError.set('');
    this.service.getAssignees().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: technicians => {
        if (this.ticket()?.ticketId !== ticket.ticketId) return;
        this.technicians.set(technicians); this.techniciansLoading.set(false);
        this.assignee.setValue(technicians.some(u => u.userId === ticket.assignedToUserId) ? ticket.assignedToUserId! : 0);
      },
      error: () => {
        if (this.ticket()?.ticketId !== ticket.ticketId) return;
        this.techniciansLoading.set(false); this.techniciansError.set('Teknisyenler yüklenemedi. Tekrar deneyin.');
      }
    });
  }

  assign(): void {
    const ticket = this.ticket();
    this.assignee.markAsTouched();
    if (!ticket || this.saving() || this.closed() || !this.auth.hasRole('Admin') || this.assignee.invalid ||
        !this.technicians().some(u => u.userId === this.assignee.value)) return;
    this.saving.set(true); this.actionError.set(''); this.success.set('');
    this.service.assignTicket(ticket, this.assignee.value).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => this.completeAction(ticket.ticketId, updated, 'Ticket teknisyene atandı.'),
      error: response => this.failAction(ticket.ticketId, response)
    });
  }

  claim(): void {
    const ticket = this.ticket();
    if (!ticket || this.saving() || !this.canClaim()) return;
    this.saving.set(true); this.actionError.set(''); this.success.set('');
    this.service.claimTicket(ticket).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => this.completeAction(ticket.ticketId, updated, 'Ticket üzerinize alındı.'),
      error: response => this.failAction(ticket.ticketId, response)
    });
  }

  changeStatus(status: TicketTargetStatus): void {
    const ticket = this.ticket();
    if (!ticket || this.saving() || !this.canChangeStatus() || ticket.assignedToUserId == null) return;
    if (status === 'In Progress' && ticket.status !== 'Open' ||
        status === 'Closed' && (ticket.status !== 'In Progress' || !this.closeRequested())) return;
    this.saving.set(true); this.actionError.set(''); this.success.set('');
    this.service.changeStatus(ticket, status).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: updated => this.completeAction(ticket.ticketId, updated, status === 'Closed' ? 'Ticket kapatıldı.' : 'Ticket üzerinde çalışmaya başlandı.'),
      error: response => this.failAction(ticket.ticketId, response)
    });
  }

  onCommentConflict(message: string): void {
    this.actionError.set(message);
    if (!this.saving()) this.refresh.next();
  }

  private completeAction(id: number, updated: Ticket, message: string): void {
    if (this.ticket()?.ticketId !== id) return;
    this.ticket.set(updated); this.saving.set(false); this.success.set(message); this.closeRequested.set(false);
  }

  private failAction(id: number, response: HttpErrorResponse): void {
    if (this.ticket()?.ticketId !== id) return;
    this.saving.set(false);
    this.closeRequested.set(false);
    this.actionError.set(typeof response.error === 'string' && [400, 409].includes(response.status)
      ? response.error : 'İşlem tamamlanamadı. Bağlantınızı ve yetkinizi kontrol edip tekrar deneyin.');
    if ([404, 409].includes(response.status)) this.refresh.next();
  }
}
