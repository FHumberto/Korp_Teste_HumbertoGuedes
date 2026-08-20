export type InvoiceStatus = 'open' | 'closed';
export interface InvoiceItem { readonly productId: string; readonly productCode: string; readonly productDescription: string; readonly quantity: number; }
export interface Invoice { readonly id: string; readonly number: number; readonly status: InvoiceStatus; readonly items: readonly InvoiceItem[]; readonly createdAt: string; readonly closedAt: string | null; }
export interface InvoiceSummary { readonly id: string; readonly number: number; readonly status: InvoiceStatus; readonly itemCount: number; readonly createdAt: string; readonly closedAt: string | null; }
export interface CreateInvoiceRequest { readonly items: readonly { readonly productId: string; readonly quantity: number }[]; }
export interface CloseInvoiceResponse { readonly id: string; readonly number: number; readonly status: 'closed'; readonly closedAt: string; }
