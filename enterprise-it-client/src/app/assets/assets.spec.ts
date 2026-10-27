import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { Assets } from './assets';

describe('Assets', () => {
  let fixture: ComponentFixture<Assets>;
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Assets],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Assets);
    http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('shows loading and renders assets returned by the API', () => {
    expect(fixture.nativeElement.textContent).toContain('Demirbaşlar yükleniyor');
    http.expectOne('/api/Asset').flush([{
      assetId: 1, assetCode: 'IT-001', brand: 'Dell', model: 'Latitude',
      serialNumber: 'SN-001', status: 'Available', assetTypeId: 1,
      purchaseDate: '2026-01-01T00:00:00Z', warrantyEndDate: '2028-01-01T00:00:00Z'
    }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('IT-001');
    expect(fixture.nativeElement.textContent).toContain('Dell Latitude');
  });

  it('shows an empty state when there are no assets', () => {
    http.expectOne('/api/Asset').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Henüz kayıtlı demirbaş bulunmuyor.');
    expect(fixture.nativeElement.querySelector('table')).toBeNull();
  });

  it('allows retry after an API error', () => {
    http.expectOne('/api/Asset').flush('Unavailable', { status: 503, statusText: 'Service Unavailable' });
    fixture.detectChanges();
    const alert = fixture.nativeElement.querySelector('[role="alert"]');
    expect(alert.textContent).toContain('Demirbaşlar yüklenemedi.');
    alert.querySelector('button').click();
    http.expectOne('/api/Asset').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });

  function prepareForm(): void {
    http.expectOne('/api/Asset').flush([]);
    fixture.componentInstance.openForm();
    http.expectOne('/api/AssetType').flush([{ assetTypeId: 1, name: 'Laptop' }]);
    fixture.componentInstance.form.setValue({
      assetCode: ' IT-007 ', brand: 'Dell', model: 'Latitude',
      serialNumber: 'SN-007', assetTypeId: 1, status: 'Available',
      purchaseDate: '2026-01-01', warrantyEndDate: '2028-01-01'
    });
    fixture.detectChanges();
  }

  it('creates an asset with UTC dates and reloads the list', () => {
    prepareForm();
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    const request = http.expectOne('/api/Asset');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.assetCode).toBe('IT-007');
    expect(request.request.body.purchaseDate).toBe('2026-01-01T00:00:00Z');
    expect(request.request.body.assetTypeId).toBe(1);
    fixture.componentInstance.saveAsset();
    http.expectNone('/api/Asset');
    const created = { ...request.request.body, assetId: 7 };
    request.flush(created);
    http.expectOne('/api/Asset').flush([created]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('IT-007 kodlu demirbaş eklendi.');
    expect(fixture.nativeElement.querySelector('tbody').textContent).toContain('IT-007');
  });

  it('blocks empty fields and warranty dates before purchase', () => {
    prepareForm();
    fixture.componentInstance.form.controls.assetCode.setValue('   ');
    fixture.componentInstance.saveAsset();
    http.expectNone('/api/Asset');
    expect(fixture.componentInstance.form.controls.assetCode.invalid).toBe(true);
    fixture.componentInstance.form.controls.assetCode.setValue('IT-007');
    fixture.componentInstance.form.controls.warrantyEndDate.setValue('2025-01-01');
    fixture.componentInstance.saveAsset();
    http.expectNone('/api/Asset');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Garanti bitişi satın alma tarihinden önce olamaz.');
  });

  it('preserves form values and displays duplicate-code errors', () => {
    prepareForm();
    fixture.componentInstance.saveAsset();
    http.expectOne('/api/Asset').flush('Bu Asset Code zaten kullanılıyor.', {
      status: 400, statusText: 'Bad Request'
    });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form [role="alert"]').textContent).toContain('Bu Asset Code zaten kullanılıyor.');
    expect(fixture.componentInstance.form.controls.assetCode.value).toBe(' IT-007 ');
    expect(fixture.componentInstance.saving()).toBe(false);
  });

  const existingAsset = { assetId: 1, assetCode: 'IT-001', brand: 'Dell', model: 'Latitude', serialNumber: 'SN-001',
    assetTypeId: 1, status: 'Available', purchaseDate: '2026-01-01T00:00:00Z', warrantyEndDate: '2028-01-01T00:00:00Z' };

  function prepareEdit(status = 'Available'): void {
    http.expectOne('/api/Asset').flush([{ ...existingAsset, status }]);
    fixture.detectChanges();
    fixture.nativeElement.querySelector('tbody button').click();
    http.expectOne('/api/AssetType').flush([{ assetTypeId: 1, name: 'Laptop' }]);
    fixture.detectChanges();
  }

  it('opens the selected asset with dates formatted for date inputs', () => {
    prepareEdit();
    expect(fixture.componentInstance.editingId()).toBe(1);
    expect(fixture.componentInstance.form.controls.purchaseDate.value).toBe('2026-01-01');
    expect(fixture.componentInstance.form.controls.warrantyEndDate.value).toBe('2028-01-01');
    expect(fixture.nativeElement.querySelector('#create-title').textContent).toContain('Demirbaşı düzenle');
  });

  it('updates using PUT and reloads the list', () => {
    prepareEdit();
    fixture.componentInstance.form.controls.model.setValue(' Updated Model ');
    fixture.componentInstance.saveAsset();
    const request = http.expectOne('/api/Asset/1');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body.model).toBe('Updated Model');
    expect(request.request.body.purchaseDate).toBe('2026-01-01T00:00:00Z');
    fixture.componentInstance.saveAsset(); http.expectNone('/api/Asset/1');
    request.flush(null);
    http.expectOne('/api/Asset').flush([{ ...existingAsset, model: 'Updated Model' }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('IT-001 kodlu demirbaş güncellendi.');
    expect(fixture.nativeElement.querySelector('tbody').textContent).toContain('Updated Model');
  });

  it('preserves an existing status outside the standard dropdown options', () => {
    prepareEdit('Assigned');
    expect(fixture.nativeElement.querySelector('#asset-status').value).toBe('Assigned');
    fixture.componentInstance.saveAsset();
    const request = http.expectOne('/api/Asset/1');
    expect(request.request.body.status).toBe('Assigned');
    request.flush(null); http.expectOne('/api/Asset').flush([]);
  });

  it('retains edit data when the code is already used', () => {
    prepareEdit();
    fixture.componentInstance.form.controls.assetCode.setValue('IT-002');
    fixture.componentInstance.saveAsset();
    http.expectOne('/api/Asset/1').flush('Bu Asset Code zaten kullanılıyor.', { status: 400, statusText: 'Bad Request' });
    expect(fixture.componentInstance.editingId()).toBe(1);
    expect(fixture.componentInstance.form.controls.assetCode.value).toBe('IT-002');
    expect(fixture.componentInstance.saveError()).toContain('zaten kullanılıyor');
  });

  it('handles a deleted record without discarding the edit form', () => {
    prepareEdit(); fixture.componentInstance.saveAsset();
    http.expectOne('/api/Asset/1').flush({}, { status: 404, statusText: 'Not Found' });
    expect(fixture.componentInstance.saveError()).toContain('artık bulunmuyor');
    expect(fixture.componentInstance.formOpen()).toBe(true);
  });

  it('clears editing state when cancelled and returns to create mode', () => {
    prepareEdit(); fixture.componentInstance.closeForm();
    http.expectNone('/api/Asset/1');
    fixture.componentInstance.openForm();
    expect(fixture.componentInstance.editingId()).toBeNull();
    expect(fixture.componentInstance.form.controls.assetCode.value).toBe('');
    expect(fixture.componentInstance.form.controls.status.value).toBe('Available');
  });
});
