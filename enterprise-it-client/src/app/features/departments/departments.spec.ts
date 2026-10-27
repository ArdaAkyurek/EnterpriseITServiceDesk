import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Departments } from './departments';

describe('Departments', () => {
  let fixture: ComponentFixture<Departments>;
  let http: HttpTestingController;
  const department = { departmentId: 1, departmentName: 'IT', departmentDescription: 'Information Technology' };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Departments], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    fixture = TestBed.createComponent(Departments); http = TestBed.inject(HttpTestingController); fixture.detectChanges();
  });
  afterEach(() => http.verify());
  function load(): void { http.expectOne('/api/Department').flush([department]); fixture.detectChanges(); }

  it('lists departments and descriptions', () => {
    load();
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Information Technology');
  });
  it('creates a department using trimmed input and reloads the list', () => {
    load(); fixture.componentInstance.openForm();
    fixture.componentInstance.form.setValue({ departmentName: ' HR ', departmentDescription: '  ' });
    fixture.componentInstance.save();
    const request = http.expectOne('/api/Department');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ departmentName: 'HR', departmentDescription: null });
    fixture.componentInstance.save(); http.expectNone('/api/Department');
    request.flush({ departmentId: 2, ...request.request.body }); load();
    expect(fixture.componentInstance.formOpen()).toBe(false);
    expect(fixture.componentInstance.success()).toContain('oluşturuldu');
  });
  it('edits an existing department using its ID', () => {
    load(); fixture.componentInstance.openForm(department);
    fixture.componentInstance.form.controls.departmentDescription.setValue('Updated'); fixture.componentInstance.save();
    const request = http.expectOne('/api/Department/1');
    expect(request.request.method).toBe('PUT'); expect(request.request.body.departmentDescription).toBe('Updated');
    request.flush(null); load();
  });
  it('retains input on duplicate-name errors', () => {
    load(); fixture.componentInstance.openForm();
    fixture.componentInstance.form.controls.departmentName.setValue('IT'); fixture.componentInstance.save();
    http.expectOne('/api/Department').flush('Bu isimde bir departman zaten var.', { status: 409, statusText: 'Conflict' });
    expect(fixture.componentInstance.saveError()).toContain('zaten var');
    expect(fixture.componentInstance.form.controls.departmentName.value).toBe('IT');
  });
  it('blocks whitespace-only names and excessive descriptions', () => {
    load(); fixture.componentInstance.openForm();
    fixture.componentInstance.form.setValue({ departmentName: '   ', departmentDescription: 'x'.repeat(251) });
    fixture.componentInstance.save(); http.expectNone('/api/Department');
    expect(fixture.componentInstance.form.invalid).toBe(true);
  });
  it('deletes only after confirmation and prevents duplicate requests', () => {
    load(); fixture.componentInstance.askDelete(department); http.expectNone('/api/Department/1');
    fixture.detectChanges(); fixture.nativeElement.querySelector('.delete-confirmation .danger').click();
    const request = http.expectOne('/api/Department/1'); expect(request.request.method).toBe('DELETE');
    fixture.componentInstance.confirmDelete(); http.expectNone('/api/Department/1');
    request.flush(null); http.expectOne('/api/Department').flush([]); fixture.detectChanges();
    expect(fixture.componentInstance.deleteTarget()).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Departman silindi.');
  });
  it('keeps the department when the server reports linked users', () => {
    load(); fixture.componentInstance.askDelete(department); fixture.componentInstance.confirmDelete();
    http.expectOne('/api/Department/1').flush('Bu departmanda kullanıcılar var.', { status: 409, statusText: 'Conflict' });
    expect(fixture.componentInstance.departments()).toEqual([department]);
    expect(fixture.componentInstance.deleteError()).toContain('kullanıcılar var');
    expect(fixture.componentInstance.deleting()).toBe(false);
  });
  it('cancels deletion without an API call', () => {
    load(); fixture.componentInstance.askDelete(department); fixture.componentInstance.cancelDelete();
    http.expectNone('/api/Department/1'); expect(fixture.componentInstance.deleteTarget()).toBeNull();
  });
  it('retries failed loading and shows an empty list', () => {
    http.expectOne('/api/Department').flush({}, { status: 503, statusText: 'Unavailable' }); fixture.detectChanges();
    fixture.nativeElement.querySelector('[role="alert"] button').click(); http.expectOne('/api/Department').flush([]); fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Henüz departman bulunmuyor.');
  });
});
