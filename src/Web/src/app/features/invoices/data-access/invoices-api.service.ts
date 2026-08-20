import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { API_ENDPOINTS } from '../../../core/config/api-endpoints';
import { CloseInvoiceResponse, CreateInvoiceRequest, Invoice, InvoiceStatus, InvoiceSummary } from '../models/invoice.models';

@Service()
export class InvoicesApiService {
  private readonly http = inject(HttpClient);
  private readonly endpoints = inject(API_ENDPOINTS);

  create(request: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.endpoints.billing}/invoices`, request);
  }

  list(status?: InvoiceStatus): Observable<readonly InvoiceSummary[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<readonly InvoiceSummary[]>(`${this.endpoints.billing}/invoices`, { params });
  }

  getById(invoiceId: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.endpoints.billing}/invoices/${invoiceId}`);
  }

  close(invoiceId: string): Observable<CloseInvoiceResponse> {
    return this.http.post<CloseInvoiceResponse>(`${this.endpoints.billing}/invoices/${invoiceId}/close`, null);
  }

  getDocument(invoiceId: string): Observable<Blob> {
    return this.http.get(`${this.endpoints.billing}/invoices/${invoiceId}/document.pdf`, { responseType: 'blob' });
  }
}
