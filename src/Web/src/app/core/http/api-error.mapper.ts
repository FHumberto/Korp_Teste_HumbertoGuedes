import { HttpErrorResponse } from '@angular/common/http';
import { ApiError, ProblemDetails } from './problem-details';

const knownCodes = new Set([
  'VALIDATION_ERROR',
  'PRODUCT_NOT_FOUND',
  'PRODUCT_CODE_ALREADY_EXISTS',
  'INVOICE_NOT_FOUND',
  'INVOICE_ALREADY_CLOSED',
  'INSUFFICIENT_STOCK',
  'IDEMPOTENCY_CONFLICT',
  'INVENTORY_UNAVAILABLE',
  'RATE_LIMIT_EXCEEDED',
  'UNEXPECTED_ERROR',
]);

export function mapApiError(error: unknown): ApiError {
  if (!(error instanceof HttpErrorResponse)) {
    return { status: 0, code: 'UNEXPECTED_ERROR', message: 'Não foi possível concluir a operação.', fieldErrors: {} };
  }

  const problem = isProblemDetails(error.error) ? error.error : undefined;
  const detailCode = problem?.detail && knownCodes.has(problem.detail) ? problem.detail : undefined;
  const code = problem?.code ?? detailCode ?? codeForStatus(error.status);

  return {
    status: error.status,
    code,
    message: messageFor(error.status, code, problem),
    traceId: problem?.traceId,
    fieldErrors: problem?.errors ?? {},
  };
}

function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null;
}

function codeForStatus(status: number): string {
  if (status === 0 || status === 503) return 'INVENTORY_UNAVAILABLE';
  if (status === 429) return 'RATE_LIMIT_EXCEEDED';
  return 'UNEXPECTED_ERROR';
}

function messageFor(status: number, code: string, problem?: ProblemDetails): string {
  if (problem?.detail && problem.detail !== code) return problem.detail;
  if (problem?.title) return problem.title;
  if (status === 0) return 'Não foi possível conectar aos serviços. Verifique se as APIs estão disponíveis.';
  if (code === 'RATE_LIMIT_EXCEEDED') return 'Muitas solicitações foram realizadas. Aguarde e tente novamente.';
  return 'Não foi possível concluir a operação.';
}
