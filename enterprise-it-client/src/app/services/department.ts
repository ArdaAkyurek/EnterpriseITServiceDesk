import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Department, SaveDepartment } from '../models/department';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Department';
  getDepartments() { return this.http.get<Department[]>(this.apiUrl); }
  createDepartment(value: SaveDepartment) { return this.http.post<Department>(this.apiUrl, value); }
  updateDepartment(id: number, value: SaveDepartment) { return this.http.put<void>(`${this.apiUrl}/${id}`, value); }
  deleteDepartment(id: number) { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
