import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { Invoice } from '../models/invoice.models';
import { InvoiceDetailsPage } from './invoice-details-page';

const openInvoice: Invoice = {
  id: 'invoice-1', number: 10, status: 'open', createdAt: '2026-08-20T10:00:00Z', closedAt: null,
  items: [{ productId: 'product-1', productCode: 'PROD-001', productDescription: 'Produto', quantity: 2 }],
};

describe('InvoiceDetailsPage', () => {
  function setup() {
    TestBed.configureTestingModule({
      imports: [InvoiceDetailsPage],
      providers: [
        provideHttpClient(), provideHttpClientTesting(), provideRouter([]),
        { provide: API_ENDPOINTS, useValue: { inventory: '', billing: 'http://billing/api/v1' } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: 'invoice-1' }) } } },
      ],
    });
    const fixture = TestBed.createComponent(InvoiceDetailsPage);
    const http = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
    return { fixture, http };
  }

  it('should close the invoice and print only after backend confirmation', async () => {
    const print = vi.spyOn(globalThis, 'print').mockImplementation(() => undefined);
    const { fixture, http } = setup();
    http.expectOne('http://billing/api/v1/invoices/invoice-1').flush(openInvoice);
    await fixture.whenStable();
    fixture.detectChanges();

    const printButton = Array.from(fixture.nativeElement.querySelectorAll('button')).find((button) => (button as HTMLButtonElement).textContent?.includes('Imprimir')) as HTMLButtonElement;
    printButton.click();
    expect(print).not.toHaveBeenCalled();
    http.expectOne('http://billing/api/v1/invoices/invoice-1/close').flush({ id: 'invoice-1', number: 10, status: 'closed', closedAt: '2026-08-20T10:05:00Z' });
    await new Promise((resolve) => globalThis.setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(print).toHaveBeenCalledOnce();
    expect(fixture.nativeElement.textContent).toContain('Nota fechada com sucesso.');
    expect(fixture.nativeElement.textContent).not.toContain('Processando fechamento...');
    http.verify();
    print.mockRestore();
  });

  it('should keep the invoice open and allow retry when inventory is unavailable', async () => {
    const print = vi.spyOn(globalThis, 'print').mockImplementation(() => undefined);
    const { fixture, http } = setup();
    http.expectOne('http://billing/api/v1/invoices/invoice-1').flush(openInvoice);
    await fixture.whenStable();
    fixture.detectChanges();

    const printButton = Array.from(fixture.nativeElement.querySelectorAll('button')).find((button) => (button as HTMLButtonElement).textContent?.includes('Imprimir')) as HTMLButtonElement;
    printButton.click();
    http.expectOne('http://billing/api/v1/invoices/invoice-1/close').flush({ title: 'O serviço de Estoque está indisponível.', status: 503, detail: 'O serviço de Estoque está indisponível.', code: 'INVENTORY_UNAVAILABLE' }, { status: 503, statusText: 'Unavailable' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(print).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('O fechamento não foi concluído.');
    expect(fixture.nativeElement.textContent).toContain('Imprimir');
    http.verify();
    print.mockRestore();
  });

  it('should not show the print action for a closed invoice', async () => {
    const { fixture, http } = setup();
    http.expectOne('http://billing/api/v1/invoices/invoice-1').flush({ ...openInvoice, status: 'closed', closedAt: '2026-08-20T10:05:00Z' });
    await fixture.whenStable();
    fixture.detectChanges();
    const buttons = Array.from(fixture.nativeElement.querySelectorAll('button')).map((button) => (button as HTMLButtonElement).textContent);
    expect(buttons.some((text) => text?.includes('Imprimir'))).toBe(false);
    http.verify();
  });
});
