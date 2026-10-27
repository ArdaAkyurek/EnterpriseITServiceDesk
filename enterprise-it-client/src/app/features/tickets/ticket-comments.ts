import { Component, DestroyRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { TicketService } from '../../services/ticket';
import { TicketComment } from '../../models/ticket';

@Component({
  selector: 'app-ticket-comments', standalone: true, imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './ticket-comments.html', styleUrls: ['../../assets/assets.css', './ticket-comments.css']
})
export class TicketComments implements OnChanges {
  @Input({ required: true }) ticketId!: number;
  @Input() closed = false;
  @Input() busy = false;
  @Output() ticketChanged = new EventEmitter<string>();
  private readonly service = inject(TicketService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private listRequest?: Subscription;
  private postRequest?: Subscription;
  readonly comments = signal<TicketComment[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly posting = signal(false);
  readonly postError = signal('');
  readonly success = signal('');
  readonly form = this.fb.nonNullable.group({ comment: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(1000)]] });

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['ticketId']) return;
    this.listRequest?.unsubscribe(); this.postRequest?.unsubscribe();
    this.comments.set([]); this.form.reset(); this.posting.set(false);
    this.postError.set(''); this.success.set('');
    this.loadComments();
  }

  loadComments(): void {
    if (this.posting()) return;
    const id = this.ticketId;
    this.listRequest?.unsubscribe(); this.loading.set(true); this.error.set('');
    this.listRequest = this.service.getComments(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: comments => { if (this.ticketId === id) { this.comments.set(comments); this.loading.set(false); } },
      error: () => { if (this.ticketId === id) { this.loading.set(false); this.error.set('Yorumlar yüklenemedi. Tekrar deneyin.'); } }
    });
  }

  sendComment(): void {
    if (this.closed || this.busy || this.posting() || this.loading() || this.error()) return;
    this.form.markAllAsTouched(); this.postError.set(''); this.success.set('');
    if (this.form.invalid) return;
    const id = this.ticketId;
    this.posting.set(true);
    this.postRequest = this.service.addComment(id, this.form.getRawValue().comment.trim())
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          if (this.ticketId !== id) return;
          this.posting.set(false); this.form.reset(); this.success.set('Yorumunuz gönderildi.'); this.loadComments();
        },
        error: (response: HttpErrorResponse) => {
          if (this.ticketId !== id) return;
          this.posting.set(false);
          this.postError.set(typeof response.error === 'string' && [400, 409].includes(response.status)
            ? response.error : 'Yorum gönderilemedi. Bağlantınızı kontrol edip tekrar deneyin.');
          if ([404, 409].includes(response.status)) this.ticketChanged.emit(this.postError());
        }
      });
  }
}
