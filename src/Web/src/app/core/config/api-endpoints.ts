import { InjectionToken } from '@angular/core';

export interface ApiEndpoints {
  readonly inventory: string;
  readonly billing: string;
}

export const API_ENDPOINTS = new InjectionToken<ApiEndpoints>('API_ENDPOINTS');
