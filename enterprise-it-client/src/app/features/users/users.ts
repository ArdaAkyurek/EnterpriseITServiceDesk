import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, forkJoin } from 'rxjs';
import { ManagedUser, UserDepartmentOption, UserRoleOption } from '../../models/user';
import { UserService } from '../../services/user';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './users.html',
  styleUrls: ['../../assets/assets.css', './users.css']
})
export class Users implements OnInit {
  private readonly service = inject(UserService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  readonly users = signal<ManagedUser[]>([]);
  readonly roles = signal<UserRoleOption[]>([]);
  readonly departments = signal<UserDepartmentOption[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly saving = signal(false);
  readonly formOpen = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saveError = signal('');
  readonly success = signal('');
  private readonly passwordValidators = [Validators.minLength(12), Validators.maxLength(128)];
  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(50)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
    phone: ['', Validators.maxLength(20)],
    password: ['', [Validators.required, ...this.passwordValidators]],
    departmentId: [0, [Validators.required, Validators.min(1)]],
    roleId: [0, [Validators.required, Validators.min(1)]],
    isActive: [true]
  });

  ngOnInit(): void { this.loadData(); }

  loadData(): void {
    if (this.loading() || this.saving()) return;
    this.loading.set(true);
    this.error.set('');
    forkJoin({ users: this.service.getUsers(), roles: this.service.getRoles(), departments: this.service.getDepartments() })
      .pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: data => {
          this.users.set(data.users);
          this.roles.set(data.roles.filter(role => ['Admin', 'Technician', 'Employee'].includes(role.roleName)));
          this.departments.set(data.departments);
          this.loading.set(false);
        },
        error: () => { this.loading.set(false); this.error.set('Kullanıcılar veya form seçenekleri yüklenemedi. Tekrar deneyin.'); }
      });
  }

  roleName(id: number): string { return this.roles().find(role => role.roleId === id)?.roleName ?? 'Tanımsız rol'; }
  departmentName(id: number): string { return this.departments().find(department => department.departmentId === id)?.departmentName ?? 'Tanımsız departman'; }
  isEditingSelf(): boolean { return this.editingId() === this.auth.user()?.userId; }

  openCreate(): void {
    if (this.saving()) return;
    this.resetForm();
    this.form.controls.password.setValidators([Validators.required, ...this.passwordValidators]);
    this.form.controls.password.updateValueAndValidity();
    this.formOpen.set(true);
  }

  openEdit(user: ManagedUser): void {
    if (this.saving()) return;
    this.resetForm();
    this.editingId.set(user.userId);
    this.form.setValue({
      firstName: user.firstName, lastName: user.lastName, email: user.email,
      phone: user.phone ?? '', password: '', departmentId: user.departmentId, roleId: user.roleId, isActive: user.isActive
    });
    this.form.controls.password.setValidators(this.passwordValidators);
    this.form.controls.password.updateValueAndValidity();
    if (this.isEditingSelf()) {
      this.form.controls.roleId.disable();
      this.form.controls.isActive.disable();
    }
    this.formOpen.set(true);
  }

  closeForm(): void {
    if (this.saving()) return;
    this.formOpen.set(false);
    this.resetForm();
  }

  private resetForm(): void {
    this.form.enable();
    this.form.reset();
    this.editingId.set(null);
    this.saveError.set('');
    this.success.set('');
  }

  invalid(field: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[field];
    return control.touched && control.invalid;
  }

  saveUser(): void {
    if (this.saving() || this.loading()) return;
    this.saveError.set('');
    this.form.markAllAsTouched();
    if (this.form.invalid || !this.roles().length || !this.departments().length) return;
    const id = this.editingId();
    const self = this.isEditingSelf();
    const { password, ...value } = this.form.getRawValue();
    const user = { ...value, firstName: value.firstName.trim(), lastName: value.lastName.trim(),
      email: value.email.trim().toLowerCase(), phone: value.phone.trim() || null };
    const request: Observable<ManagedUser | void> = id === null ? this.service.createUser({ ...user, password })
      : this.service.updateUser(id, { ...user, ...(password ? { password } : {}) });
    this.saving.set(true);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeForm();
        if (self && password) {
          this.auth.clearSession();
          void this.router.navigate(['/'], { queryParams: { reason: 'password-changed' } });
          return;
        }
        if (self) this.auth.user.update(current => current ? { ...current, firstName: user.firstName, lastName: user.lastName, email: user.email } : null);
        this.success.set(id === null ? 'Kullanıcı oluşturuldu.' : 'Kullanıcı güncellendi.');
        this.loadData();
      },
      error: (response: HttpErrorResponse) => {
        this.saving.set(false);
        this.saveError.set(response.status === 400 && typeof response.error === 'string' ? response.error
          : 'Kullanıcı kaydedilemedi. Bilgileri ve bağlantıyı kontrol edip tekrar deneyin.');
      }
    });
  }
}
