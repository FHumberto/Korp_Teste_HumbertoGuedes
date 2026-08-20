export const PRODUCT_LIMITS = { code: 50, description: 200, maxPageSize: 100 } as const;

export interface ProductSummary { readonly id: string; readonly code: string; readonly description: string; readonly balance: number; }
export interface Product extends ProductSummary { readonly createdAt: string; readonly updatedAt: string | null; }
export interface CreateProductRequest { readonly code: string; readonly description: string; readonly initialBalance: number; }
export interface CreatedProduct extends ProductSummary { readonly createdAt: string; }
export interface Paged<T> { readonly items: readonly T[]; readonly totalRecords: number; readonly pageNumber: number; readonly pageSize: number; readonly totalPages: number; }
