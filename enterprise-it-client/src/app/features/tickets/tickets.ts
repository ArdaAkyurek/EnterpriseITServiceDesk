import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Ticket, TicketFormOptions, TicketListView } from '../../models/ticket';
import { TicketService } from '../../services/ticket';
import { AuthService } from '../../services/auth';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-tickets',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule, RouterLink],
  templateUrl: './tickets.html',
  styleUrls: ['../../assets/assets.css', './tickets.css']
})
export class Tickets implements OnInit {
  private readonly service = inject(TicketService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);
  private listRequest?: Subscription;
  private readonly selectedView = signal<TicketListView>('all');
  readonly currentView = computed<TicketListView>(() => this.auth.hasRole('Employee') ? 'created' : this.selectedView());
  readonly viewLabel = computed(() => ({ all: 'Tümü', assigned: 'Bana atananlar', unassigned: 'Atanmamışlar', created: 'Taleplerim' })[this.currentView()]);
  readonly emptyMessage = computed(() => ({
    all: 'Henüz ticket bulunmuyor. Yeni bir destek talebi oluşturabilirsiniz.',
    assigned: 'Size atanmış ticket bulunmuyor.',
    unassigned: 'Atanmamış ticket bulunmuyor.',
    created: 'Henüz oluşturduğunuz bir talep bulunmuyor.'
  })[this.currentView()]);
  readonly tickets = signal<Ticket[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly formOpen = signal(false);
  readonly saving = signal(false);
  readonly options = signal<TicketFormOptions | null>(null);
  readonly optionsLoading = signal(false);
  readonly optionsError = signal('');
  readonly saveError = signal('');
  readonly success = signal('');
  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(150)]],
    description: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(10000)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    priorityId: [0, [Validators.required, Validators.min(1)]],
    assetId: this.fb.control<number | null>(null)
  });

  ngOnInit(): void { this.loadTickets(); }

  loadTickets(): void {
    if (this.saving()) return;
    this.listRequest?.unsubscribe();
    this.loading.set(true);
    this.error.set('');
    this.tickets.set([]);
    this.listRequest = this.service.getTickets(this.currentView()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: tickets => { this.tickets.set(tickets); this.loading.set(false); },
      error: () => { this.error.set('Ticket listesi yüklenemedi. Tekrar deneyin.'); this.loading.set(false); }
    });
  }

  changeView(view: TicketListView): void {
    if (this.saving() || !this.auth.hasRole('Admin', 'Technician') || view === this.currentView()) return;
    this.selectedView.set(view);
    this.loadTickets();
  }

  openForm(): void {
    this.formOpen.set(true);
    this.success.set('');
    if (!this.options()) this.loadOptions();
  }

  closeForm(): void {
    if (this.saving()) return;
    this.formOpen.set(false);
    this.form.reset();
    this.saveError.set('');
  }

  loadOptions(): void {
    if (this.optionsLoading()) return;
    this.optionsLoading.set(true);
    this.optionsError.set('');
    this.service.getFormOptions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: options => {
        this.options.set(options);
        this.optionsLoading.set(false);
        if (!options.categories.length || !options.priorities.length)
          this.optionsError.set('Ticket oluşturmak için kategori ve öncelik tanımlanmalı. Yöneticiyle iletişime geçin.');
      },
      error: () => {
        this.optionsLoading.set(false);
        this.optionsError.set('Form seçenekleri yüklenemedi. Tekrar deneyin.');
      }
    });
  }

  invalid(field: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[field];
    return control.touched && control.invalid;
  }

  saveTicket(): void {
    if (this.saving() || this.loading()) return;
    this.saveError.set('');
    this.form.markAllAsTouched();
    if (this.form.invalid || this.optionsLoading() || this.optionsError() || !this.options()) return;
    this.saving.set(true);
    const value = this.form.getRawValue();
    this.service.createTicket({ ...value, title: value.title.trim(), description: value.description.trim() })
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: ticket => {
          this.saving.set(false);
          this.closeForm();
          this.success.set(`#${ticket.ticketId} numaralı ticket oluşturuldu.`);
          this.loadTickets();
        },
        error: (response: HttpErrorResponse) => {
          this.saving.set(false);
          this.saveError.set(response.status === 400 && typeof response.error === 'string'
            ? response.error : 'Ticket kaydedilemedi. Bilgileri ve bağlantıyı kontrol edip tekrar deneyin.');
        }
      });
  }
}
