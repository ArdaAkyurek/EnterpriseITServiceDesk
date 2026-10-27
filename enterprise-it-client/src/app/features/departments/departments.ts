import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';
import { Department } from '../../models/department';
import { DepartmentService } from '../../services/department';

@Component({
  selector: 'app-departments', standalone: true, imports: [ReactiveFormsModule],
  templateUrl: './departments.html', styleUrls: ['../../assets/assets.css', './departments.css']
})
export class Departments implements OnInit {
  private readonly service = inject(DepartmentService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  readonly departments = signal<Department[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly formOpen = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal('');
  readonly deleteTarget = signal<Department | null>(null);
  readonly deleting = signal(false);
  readonly deleteError = signal('');
  readonly success = signal('');
  readonly busy = computed(() => this.saving() || this.deleting());
  readonly form = this.fb.nonNullable.group({
    departmentName: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    departmentDescription: ['', Validators.maxLength(250)]
  });

  ngOnInit(): void { this.loadDepartments(); }

  loadDepartments(): void {
    if (this.loading() || this.busy()) return;
    this.loading.set(true); this.error.set('');
    this.service.getDepartments().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: departments => { this.departments.set(departments); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Departmanlar yüklenemedi. Tekrar deneyin.'); }
    });
  }

  openForm(department?: Department): void {
    if (this.busy() || this.formOpen() || this.deleteTarget()) return;
    this.editingId.set(department?.departmentId ?? null);
    this.form.reset({ departmentName: department?.departmentName ?? '', departmentDescription: department?.departmentDescription ?? '' });
    this.saveError.set(''); this.success.set(''); this.formOpen.set(true);
  }

  closeForm(): void {
    if (this.busy()) return;
    this.formOpen.set(false); this.form.reset(); this.editingId.set(null); this.saveError.set('');
  }

  save(): void {
    if (this.busy() || this.loading() || !this.formOpen()) return;
    this.form.markAllAsTouched(); this.saveError.set('');
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    const value = { departmentName: raw.departmentName.trim(), departmentDescription: raw.departmentDescription.trim() || null };
    const id = this.editingId();
    const request: Observable<Department | void> = id === null ? this.service.createDepartment(value) : this.service.updateDepartment(id, value);
    this.saving.set(true);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false); this.closeForm();
        this.success.set(id === null ? 'Departman oluşturuldu.' : 'Departman güncellendi.'); this.loadDepartments();
      },
      error: (response: HttpErrorResponse) => {
        this.saving.set(false);
        this.saveError.set(response.status === 404 ? 'Departman artık bulunmuyor. Listeyi yenileyin.'
          : typeof response.error === 'string' && [400, 409].includes(response.status) ? response.error
          : 'Departman kaydedilemedi. Bilgileri ve bağlantıyı kontrol edip tekrar deneyin.');
      }
    });
  }

  askDelete(department: Department): void {
    if (this.busy() || this.formOpen() || this.deleteTarget()) return;
    this.success.set(''); this.deleteError.set(''); this.deleteTarget.set(department);
  }

  cancelDelete(): void {
    if (this.busy()) return;
    this.deleteTarget.set(null); this.deleteError.set('');
  }

  confirmDelete(): void {
    const department = this.deleteTarget();
    if (!department || this.busy() || this.loading()) return;
    this.deleting.set(true); this.deleteError.set('');
    this.service.deleteDepartment(department.departmentId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.deleting.set(false); this.cancelDelete(); this.success.set('Departman silindi.'); this.loadDepartments();
      },
      error: (response: HttpErrorResponse) => {
        this.deleting.set(false);
        if (response.status === 404) {
          this.cancelDelete(); this.success.set('Departman zaten kaldırılmış. Liste güncellendi.'); this.loadDepartments();
        } else this.deleteError.set(typeof response.error === 'string' && response.status === 409 ? response.error
          : 'Departman silinemedi. Bağlantınızı kontrol edip tekrar deneyin.');
      }
    });
  }
}
