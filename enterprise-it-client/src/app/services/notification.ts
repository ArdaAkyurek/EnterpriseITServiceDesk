import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NotificationPage } from '../models/notification';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly http = inject(HttpClient);
  getPage(page: number) { return this.http.get<NotificationPage>(`/api/Notification?page=${page}`); }
  markRead(id: number) { return this.http.put<void>(`/api/Notification/${id}/read`, {}); }
}
