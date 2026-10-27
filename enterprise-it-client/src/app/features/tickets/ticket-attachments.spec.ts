import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TicketAttachments } from './ticket-attachments';

describe('TicketAttachments', () => {
  let fixture: ComponentFixture<TicketAttachments>;
  let http: HttpTestingController;
  const attachment = { attachmentId: 3, ticketId: 42, fileName: 'report.txt', uploadedBy: 'Test User', createdAt: '2026-10-27T09:00:00Z' };
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [TicketAttachments], providers: [provideHttpClient(), provideHttpClientTesting()] }).compileComponents();
    fixture = TestBed.createComponent(TicketAttachments); http = TestBed.inject(HttpTestingController);
    fixture.componentRef.setInput('ticketId', 42); fixture.detectChanges();
  });
  afterEach(() => { http.verify(); fixture.destroy(); vi.restoreAllMocks(); vi.useRealTimers(); });
  function select(file: File) { fixture.componentInstance.selectFile({ target: { files: [file] } } as unknown as Event); }
  function load() { http.expectOne('/api/TicketAttachment?ticketId=42').flush([attachment]); fixture.detectChanges(); }
  it('lists filename, author and download action', () => {
    load(); expect(fixture.nativeElement.textContent).toContain('report.txt'); expect(fixture.nativeElement.textContent).toContain('Test User');
    expect(fixture.nativeElement.querySelector('li button').textContent).toContain('İndir');
  });
  it('uploads multipart bytes with ticket ID and refreshes after success', () => {
    load(); const file = new File(['hello'], 'hello.txt', { type: 'text/plain' }); select(file);
    fixture.componentInstance.upload(); fixture.componentInstance.upload();
    const request = http.expectOne('/api/TicketAttachment'); expect(request.request.method).toBe('POST');
    expect(request.request.body instanceof FormData).toBe(true); expect(request.request.body.get('ticketId')).toBe('42');
    expect(request.request.body.get('file').name).toBe('hello.txt'); expect(request.request.headers.has('Content-Type')).toBe(false);
    request.flush(attachment); http.expectOne('/api/TicketAttachment?ticketId=42').flush([attachment]);
    expect(fixture.componentInstance.selectedFile()).toBeNull(); expect(fixture.componentInstance.success()).toBe('Dosya eklendi.');
  });
  it('rejects empty, oversized and unsupported files without sending requests', () => {
    load();
    for (const file of [new File([], 'empty.txt'), new File([new Uint8Array(10 * 1024 * 1024 + 1)], 'large.pdf'), new File(['test'], 'script.html')]) {
      select(file); fixture.componentInstance.upload(); expect(fixture.componentInstance.selectedFile()).toBeNull(); expect(fixture.componentInstance.actionError()).not.toBe('');
    }
    http.expectNone('/api/TicketAttachment');
  });
  it('keeps the selected file on a failed upload and permits retry', () => {
    load(); select(new File(['test'], 'report.txt')); fixture.componentInstance.upload();
    http.expectOne('/api/TicketAttachment').flush('Dosya geçersiz.', { status: 400, statusText: 'Bad Request' });
    expect(fixture.componentInstance.selectedFile()?.name).toBe('report.txt'); expect(fixture.componentInstance.actionError()).toBe('Dosya geçersiz.');
    fixture.componentInstance.upload(); http.expectOne('/api/TicketAttachment').flush(attachment); http.expectOne('/api/TicketAttachment?ticketId=42').flush([attachment]);
  });
  it('reloads the parent after a closure conflict', () => {
    load(); const emit = vi.spyOn(fixture.componentInstance.ticketChanged, 'emit'); select(new File(['test'], 'report.txt')); fixture.componentInstance.upload();
    http.expectOne('/api/TicketAttachment').flush('Ticket kapalı.', { status: 409, statusText: 'Conflict' }); expect(emit).toHaveBeenCalledWith('Ticket kapalı.');
  });
  it('keeps download available for closed tickets and prevents upload', () => {
    load(); fixture.componentRef.setInput('closed', true); fixture.detectChanges(); select(new File(['test'], 'report.txt')); fixture.componentInstance.upload();
    expect(fixture.nativeElement.querySelector('input[type="file"]')).toBeNull(); expect(fixture.nativeElement.querySelector('li button').disabled).toBe(false); http.expectNone('/api/TicketAttachment');
  });
  it('blocks upload while the parent is saving', () => {
    load(); fixture.componentRef.setInput('busy', true); fixture.detectChanges(); select(new File(['test'], 'report.txt')); fixture.componentInstance.upload(); http.expectNone('/api/TicketAttachment');
  });
  it('downloads bytes through HttpClient and uses an object URL', () => {
    vi.useFakeTimers();
    load(); const create = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:test'); const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {});
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {});
    fixture.componentInstance.download(attachment); const request = http.expectOne('/api/TicketAttachment/3/download'); expect(request.request.responseType).toBe('blob');
    request.flush(new Blob(['hello'])); expect(create).toHaveBeenCalled(); expect(click).toHaveBeenCalled(); expect(fixture.componentInstance.downloadingId()).toBeNull();
    vi.advanceTimersByTime(1000); expect(revoke).toHaveBeenCalledWith('blob:test');
  });
  it('shows download failures without altering the attachment list', () => {
    load(); fixture.componentInstance.download(attachment); http.expectOne('/api/TicketAttachment/3/download').flush(null, { status: 404, statusText: 'Not Found' });
    expect(fixture.componentInstance.actionError()).toContain('indirilemedi'); expect(fixture.componentInstance.attachments()).toEqual([attachment]);
  });
  it('cancels previous ticket requests and clears the selected file on navigation', () => {
    const previous = http.expectOne('/api/TicketAttachment?ticketId=42'); select(new File(['test'], 'report.txt'));
    fixture.componentRef.setInput('ticketId', 43); fixture.detectChanges(); expect(previous.cancelled).toBe(true); expect(fixture.componentInstance.selectedFile()).toBeNull();
    http.expectOne('/api/TicketAttachment?ticketId=43').flush([]); expect(fixture.componentInstance.attachments()).toEqual([]);
  });
  it('renders server filenames as text', () => {
    http.expectOne('/api/TicketAttachment?ticketId=42').flush([{ ...attachment, fileName: '<img src=x onerror=alert(1)>.txt' }]); fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img')).toBeNull(); expect(fixture.nativeElement.textContent).toContain('<img');
  });
});
