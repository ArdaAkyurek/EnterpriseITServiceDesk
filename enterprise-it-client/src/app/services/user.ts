import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateUser, ManagedUser, UpdateUser, UserDepartmentOption, UserRoleOption } from '../models/user';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  getUsers() { return this.http.get<ManagedUser[]>('/api/User'); }
  getRoles() { return this.http.get<UserRoleOption[]>('/api/Role'); }
  getDepartments() { return this.http.get<UserDepartmentOption[]>('/api/Department'); }
  createUser(user: CreateUser) { return this.http.post<ManagedUser>('/api/User', user); }
  updateUser(id: number, user: UpdateUser) { return this.http.put<void>(`/api/User/${id}`, user); }
}
