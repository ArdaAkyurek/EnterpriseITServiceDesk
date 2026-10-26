export interface Asset {
  assetId: number;
  assetCode: string;
  brand: string;
  model: string;
  serialNumber: string;
  purchaseDate: string;
  warrantyEndDate: string;
  status: string;
  assetTypeId: number;
}

export type CreateAsset = Omit<Asset, 'assetId'>;

export interface AssetType {
  assetTypeId: number;
  name: string;
}
