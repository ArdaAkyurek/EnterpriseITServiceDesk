import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Asset, AssetType, CreateAsset } from '../models/asset';

@Injectable({
  providedIn: 'root'
})
export class AssetService {

  private readonly apiUrl = '/api/Asset';

  constructor(private http: HttpClient) {}

  getAssets() {
    return this.http.get<Asset[]>(this.apiUrl);
  }

  getAssetTypes() {
    return this.http.get<AssetType[]>('/api/AssetType');
  }

  createAsset(asset: CreateAsset) {
    return this.http.post<Asset>(this.apiUrl, asset);
  }
  updateAsset(id: number, asset: CreateAsset) {
    return this.http.put<void>(`${this.apiUrl}/${id}`, asset);
  }
}
