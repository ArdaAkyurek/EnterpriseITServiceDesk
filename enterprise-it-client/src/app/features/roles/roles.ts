import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserService } from '../../services/user';
import { UserRoleOption } from '../../models/user';
import { UserRole } from '../../models/auth';

@Component({
  selector: 'app-roles', standalone: true, imports: [RouterLink],
  templateUrl: './roles.html', styleUrl: './roles.css'
})
export class Roles implements OnInit {
  private readonly service = inject(UserService);
  private readonly destroyRef = inject(DestroyRef);
  readonly roles = signal<UserRoleOption[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly descriptions: Record<UserRole, { title: string; summary: string }> = {
    Admin: { title: 'Yönetici', summary: 'Kullanıcıları ve kurumsal tanımları yönetir; ticket’ları aktif teknisyenlere atar.' },
    Technician: { title: 'Teknisyen', summary: 'Destek taleplerini ve demirbaşları takip eder; atanmamış açık ticket’ları üzerine alır.' },
    Employee: { title: 'Çalışan', summary: 'Destek talebi oluşturur, kendi taleplerini takip eder ve profilini yönetir.' }
  };
  readonly permissions: { label: string; access: Record<UserRole, string> }[] = [
    { label: 'Ticket listeleme ve detay', access: { Admin: 'Tüm ticket’lar', Technician: 'Tüm ticket’lar', Employee: 'Kendi talepleri' } },
    { label: 'Ticket oluşturma', access: { Admin: 'Var', Technician: 'Var', Employee: 'Var' } },
    { label: 'Teknisyene atama / yeniden atama', access: { Admin: 'Var', Technician: 'Yok', Employee: 'Yok' } },
    { label: 'Atanmamış ticket’ı üzerine alma', access: { Admin: 'Yok', Technician: 'Var', Employee: 'Yok' } },
    { label: 'Ticket başlatma ve kapatma', access: { Admin: 'Tüm ticket’lar', Technician: 'Kendisine atananlar', Employee: 'Yok' } },
    { label: 'Demirbaş listeleme, ekleme ve düzenleme', access: { Admin: 'Var', Technician: 'Var', Employee: 'Yok' } },
    { label: 'Kullanıcı yönetimi', access: { Admin: 'Var', Technician: 'Yok', Employee: 'Yok' } },
    { label: 'Departman yönetimi', access: { Admin: 'Var', Technician: 'Yok', Employee: 'Yok' } },
    { label: 'Rol ekranını görüntüleme', access: { Admin: 'Var', Technician: 'Yok', Employee: 'Yok' } },
    { label: 'Audit kayıtlarını görüntüleme', access: { Admin: 'Var', Technician: 'Yok', Employee: 'Yok' } },
    { label: 'Bildirimleri görüntüleme ve okundu işaretleme', access: { Admin: 'Kendi bildirimleri', Technician: 'Kendi bildirimleri', Employee: 'Kendi bildirimleri' } },
    { label: 'Ticket dosya ekleri', access: { Admin: 'Tüm ticket’lar', Technician: 'Tüm ticket’lar', Employee: 'Kendi talepleri' } },
    { label: 'Profil, telefon ve parola', access: { Admin: 'Kendi hesabı', Technician: 'Kendi hesabı', Employee: 'Kendi hesabı' } }
  ];

  ngOnInit(): void { this.loadRoles(); }

  loadRoles(): void {
    if (this.loading()) return;
    this.loading.set(true); this.error.set('');
    this.service.getRoles().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: roles => { this.roles.set(roles); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Roller yüklenemedi. Tekrar deneyin.'); }
    });
  }

  description(name: string): { title: string; summary: string } | undefined {
    return Object.hasOwn(this.descriptions, name) ? this.descriptions[name as UserRole] : undefined;
  }

  access(row: { access: Record<UserRole, string> }, name: string): string {
    return this.description(name) ? row.access[name as UserRole] : 'Desteklenmiyor';
  }
}
