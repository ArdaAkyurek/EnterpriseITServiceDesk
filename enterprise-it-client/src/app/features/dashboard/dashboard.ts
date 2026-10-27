import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../services/auth';
import { DashboardService } from '../../services/dashboard';
import { DashboardSummary } from '../../models/dashboard';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  readonly auth = inject(AuthService);
  private readonly service = inject(DashboardService);
  private readonly destroyRef = inject(DestroyRef);
  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly scopeLabel = computed(() => ({ all: 'Tüm ticket’lar', assigned: 'Bana atanan ticket’lar', created: 'Oluşturduğum talepler' })[this.summary()?.scope ?? 'created']);
  readonly activeCount = computed(() => {
    const data = this.summary(); return data ? data.total - data.closed : 0;
  });
  readonly statuses = computed(() => {
    const data = this.summary();
    return data ? [
      { label: 'Açık', count: data.open, color: '#2563eb' },
      { label: 'İşlemde', count: data.inProgress, color: '#b45309' },
      { label: 'Kapalı', count: data.closed, color: '#15803d' },
      ...(data.other ? [{ label: 'Diğer', count: data.other, color: '#64748b' }] : [])
    ] : [];
  });

  ngOnInit(): void { this.load(); }
  load(): void {
    if (this.loading()) return;
    this.loading.set(true); this.error.set('');
    this.service.getSummary().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: summary => { this.summary.set(summary); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Dashboard verileri yüklenemedi. Tekrar deneyin.'); }
    });
  }
  statusLabel(value: string): string {
    return ({ Open: 'Açık', 'In Progress': 'İşlemde', Closed: 'Kapalı' } as Record<string, string>)[value] ?? value;
  }
}
