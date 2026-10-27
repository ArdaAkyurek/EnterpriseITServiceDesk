import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { interval } from 'rxjs';
import { NotificationService } from '../../services/notification';
import { NotificationItem } from '../../models/notification';

@Component({
  selector: 'app-notifications', standalone: true, imports: [DatePipe],
  templateUrl: './notifications.html', styleUrl: './notifications.css'
})
export class Notifications implements OnInit {
  private readonly service = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly open = signal(false);
  readonly loading = signal(false);
  readonly savingId = signal<number | null>(null);
  readonly error = signal('');
  readonly items = signal<NotificationItem[]>([]);
  readonly unreadCount = signal(0);
  readonly page = signal(1);
  readonly hasMore = signal(false);
  private requestedPage = 1;

  ngOnInit(): void {
    this.load(1);
    interval(60000).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      if (!this.open()) this.load(1);
    });
  }

  toggle(): void {
    this.open.update(value => !value);
    if (this.open()) this.load(1);
  }

  load(page = this.requestedPage): void {
    if (this.loading() || this.savingId() !== null) return;
    this.requestedPage = page;
    this.loading.set(true); this.error.set('');
    this.service.getPage(page).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: result => {
        this.items.set(result.items); this.page.set(result.page);
        this.hasMore.set(result.hasMore); this.unreadCount.set(result.unreadCount);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.error.set('Bildirimler yüklenemedi. Tekrar deneyin.'); }
    });
  }

  markRead(item: NotificationItem): void {
    if (item.isRead || this.savingId() !== null || this.loading()) return;
    this.savingId.set(item.notificationId); this.error.set('');
    this.service.markRead(item.notificationId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => { this.savingId.set(null); this.load(this.page()); },
      error: () => { this.savingId.set(null); this.error.set('Bildirim okundu olarak işaretlenemedi. Tekrar deneyin.'); }
    });
  }
}
