import { Component, DestroyRef, ElementRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subscription } from 'rxjs';
import { TicketAttachmentService } from '../../services/ticket-attachment';
import { TicketAttachment } from '../../models/ticket-attachment';

@Component({
  selector: 'app-ticket-attachments', standalone: true, imports: [DatePipe],
  templateUrl: './ticket-attachments.html', styleUrls: ['../../assets/assets.css', './ticket-attachments.css']
})
export class TicketAttachments implements OnChanges {
  @Input({ required: true }) ticketId!: number;
  @Input() closed = false;
  @Input() busy = false;
  @Output() ticketChanged = new EventEmitter<string>();
  @ViewChild('fileInput') private fileInput?: ElementRef<HTMLInputElement>;
  private readonly service = inject(TicketAttachmentService);
  private readonly destroyRef = inject(DestroyRef);
  private listRequest?: Subscription;
  private uploadRequest?: Subscription;
  private downloadRequest?: Subscription;
  readonly attachments = signal<TicketAttachment[]>([]);
  readonly selectedFile = signal<File | null>(null);
  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly downloadingId = signal<number | null>(null);
  readonly error = signal('');
  readonly actionError = signal('');
  readonly success = signal('');

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['ticketId']) return;
    this.listRequest?.unsubscribe(); this.uploadRequest?.unsubscribe(); this.downloadRequest?.unsubscribe();
    this.attachments.set([]); this.resetFile(); this.uploading.set(false); this.downloadingId.set(null);
    this.actionError.set(''); this.success.set(''); this.load();
  }

  selectFile(event: Event): void {
    this.actionError.set(''); this.success.set('');
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    this.selectedFile.set(null);
    if (!file) return;
    if (file.size === 0 || file.size > 10 * 1024 * 1024) {
      this.actionError.set('Dosya boş olamaz ve 10 MB sınırını aşamaz.'); this.resetFile(); return;
    }
    if (!/\.(pdf|png|jpe?g|txt)$/i.test(file.name)) {
      this.actionError.set('Yalnızca PDF, PNG, JPG ve TXT dosyaları seçilebilir.'); this.resetFile(); return;
    }
    this.selectedFile.set(file);
  }

  load(): void {
    if (this.uploading()) return;
    const id = this.ticketId;
    this.listRequest?.unsubscribe(); this.loading.set(true); this.error.set('');
    this.listRequest = this.service.getAttachments(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: items => { if (this.ticketId === id) { this.attachments.set(items); this.loading.set(false); } },
      error: () => { if (this.ticketId === id) { this.loading.set(false); this.error.set('Dosya ekleri yüklenemedi. Tekrar deneyin.'); } }
    });
  }

  upload(): void {
    const file = this.selectedFile();
    if (!file || this.closed || this.busy || this.uploading() || this.loading() || this.error()) return;
    const id = this.ticketId;
    this.uploading.set(true); this.actionError.set(''); this.success.set('');
    this.uploadRequest = this.service.upload(id, file).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        if (this.ticketId !== id) return;
        this.uploading.set(false); this.resetFile(); this.success.set('Dosya eklendi.'); this.load();
      },
      error: (response: HttpErrorResponse) => {
        if (this.ticketId !== id) return;
        this.uploading.set(false);
        this.actionError.set(typeof response.error === 'string' && [400, 409].includes(response.status)
          ? response.error : response.status === 413 ? 'Dosya 10 MB sınırını aşamaz.'
          : response.status === 429 ? 'Çok fazla yükleme denemesi yaptınız. Bir dakika sonra tekrar deneyin.'
          : 'Dosya kaydı tamamlanamadı. Tekrar yüklemeden önce ek listesini yenileyin.');
        if ([404, 409].includes(response.status)) this.ticketChanged.emit(this.actionError());
      }
    });
  }

  download(item: TicketAttachment): void {
    if (this.downloadingId() !== null) return;
    const id = this.ticketId;
    this.downloadingId.set(item.attachmentId); this.actionError.set('');
    this.downloadRequest = this.service.download(item.attachmentId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: blob => {
        if (this.ticketId !== id) return;
        this.downloadingId.set(null);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url; link.download = item.fileName; document.body.appendChild(link); link.click(); link.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
      },
      error: () => { if (this.ticketId === id) { this.downloadingId.set(null); this.actionError.set('Dosya indirilemedi. Dosya kaldırılmış veya erişiminiz değişmiş olabilir.'); } }
    });
  }

  private resetFile(): void {
    this.selectedFile.set(null);
    if (this.fileInput) this.fileInput.nativeElement.value = '';
  }
}
