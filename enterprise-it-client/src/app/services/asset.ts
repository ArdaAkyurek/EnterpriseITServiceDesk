import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class AssetService {

  private apiUrl = 'https://localhost:7013/api/Asset';

  constructor(private http: HttpClient) {}

  getAssets() {
    return this.http.get<any[]>(this.apiUrl);
  }
}