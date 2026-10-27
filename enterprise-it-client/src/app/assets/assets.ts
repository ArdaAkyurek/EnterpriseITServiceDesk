import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Asset, AssetType } from '../models/asset';
import { AssetService } from '../services/asset';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-assets',
  standalone: true,
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './assets.html',
  styleUrl: './assets.css'
})
export class Assets implements OnInit {
  private readonly assetService = inject(AssetService);
  private readonly destroyRef = inject(DestroyRef);

  readonly assets = signal<Asset[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly formOpen = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly knownStatuses = ['Available', 'In Use', 'Maintenance', 'Retired'];
  readonly saving = signal(false);
  readonly typesLoading = signal(false);
  readonly assetTypes = signal<AssetType[]>([]);
  readonly typesError = signal('');
  readonly saveError = signal('');
  readonly success = signal('');
  readonly submitted = signal(false);

  private readonly fb = inject(FormBuilder);
  private readonly requiredText = (max: number) => [Validators.required, Validators.pattern(/\S/), Validators.maxLength(max)];

  readonly form = this.fb.nonNullable.group({
    assetCode: ['', this.requiredText(50)],
    brand: ['', this.requiredText(100)],
    model: ['', this.requiredText(100)],
    serialNumber: ['', this.requiredText(100)],
    assetTypeId: [0, [Validators.required, Validators.min(1)]],
    status: ['Available', this.requiredText(50)],
    purchaseDate: ['', Validators.required],
    warrantyEndDate: ['', Validators.required]
  }, { validators: (control: AbstractControl): ValidationErrors | null => {
    const { purchaseDate, warrantyEndDate } = control.value;
    return purchaseDate && warrantyEndDate && warrantyEndDate < purchaseDate
      ? { warrantyBeforePurchase: true } : null;
  }});

  openForm(asset?: Asset): void {
    if (this.saving() || this.formOpen()) return;
    this.editingId.set(asset?.assetId ?? null);
    if (asset) {
      this.form.reset({
        assetCode: asset.assetCode, brand: asset.brand, model: asset.model, serialNumber: asset.serialNumber,
        assetTypeId: asset.assetTypeId, status: asset.status,
        purchaseDate: asset.purchaseDate.slice(0, 10), warrantyEndDate: asset.warrantyEndDate.slice(0, 10)
      });
    } else this.form.reset();
    this.submitted.set(false);
    this.saveError.set('');
    this.formOpen.set(true);
    this.success.set('');
    if (!this.assetTypes().length) this.loadAssetTypes();
  }

  closeForm(): void {
    if (this.saving()) return;
    this.formOpen.set(false);
    this.editingId.set(null);
    this.form.reset();
    this.submitted.set(false);
    this.saveError.set('');
  }

  loadAssetTypes(): void {
    if (this.typesLoading()) return;
    this.typesLoading.set(true);
    this.typesError.set('');
    this.assetService.getAssetTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: types => {
          this.assetTypes.set(types);
          this.typesLoading.set(false);
          if (!types.length) this.typesError.set('Kayıtlı demirbaş türü bulunmuyor. Önce bir tür tanımlanmalı.');
        },
        error: () => {
          this.typesLoading.set(false);
          this.typesError.set('Demirbaş türleri yüklenemedi. Tekrar deneyin.');
        }
      });
  }

  invalid(field: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[field];
    return control.invalid && (control.touched || this.submitted());
  }

  saveAsset(): void {
    if (this.saving() || this.loading()) return;
    this.submitted.set(true);
    this.saveError.set('');
    if (this.form.invalid || this.typesLoading() || !this.assetTypes().length) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.saving.set(true);
    const id = this.editingId();
    const payload = {
      ...value,
      assetCode: value.assetCode.trim(), brand: value.brand.trim(),
      model: value.model.trim(), serialNumber: value.serialNumber.trim(),
      purchaseDate: `${value.purchaseDate}T00:00:00Z`,
      warrantyEndDate: `${value.warrantyEndDate}T00:00:00Z`
    };
    const request: Observable<Asset | void> = id === null
      ? this.assetService.createAsset(payload) : this.assetService.updateAsset(id, payload);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeForm();
        this.success.set(`${payload.assetCode} kodlu demirbaş ${id === null ? 'eklendi' : 'güncellendi'}.`);
        this.loadAssets();
      },
      error: (response: HttpErrorResponse) => {
        this.saving.set(false);
        this.saveError.set(response.status === 404 ? 'Demirbaş artık bulunmuyor. Listeyi yenileyin.'
          : response.status === 400 && typeof response.error === 'string'
          ? response.error
          : 'Demirbaş kaydedilemedi. Bilgilerinizi ve bağlantıyı kontrol edip tekrar deneyin.');
      }
    });
  }

  ngOnInit(): void {
    this.loadAssets();
  }

  loadAssets(): void {
    if (this.loading() || this.saving()) return;

    this.loading.set(true);
    this.error.set('');

    this.assetService.getAssets()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: assets => {
          this.assets.set(assets);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Demirbaşlar yüklenemedi. Backend bağlantısını kontrol edip tekrar deneyin.');
          this.loading.set(false);
        }
      });
  }
}
