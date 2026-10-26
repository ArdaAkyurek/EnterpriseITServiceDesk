import { TestBed } from '@angular/core/testing';

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AssetService } from './asset';

describe('AssetService', () => {
  let service: AssetService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AssetService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads assets using the relative API endpoint', () => {
    service.getAssets().subscribe(assets => expect(assets).toEqual([]));
    const request = http.expectOne('/api/Asset');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });
});
