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
});
