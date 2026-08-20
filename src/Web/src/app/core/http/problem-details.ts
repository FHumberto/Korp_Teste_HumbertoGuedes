export interface ProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly detail?: string;
  readonly instance?: string;
  readonly code?: string;
  readonly traceId?: string;
  readonly errors?: Readonly<Record<string, readonly string[]>>;
}

export interface ApiError {
  readonly status: number;
  readonly code: string;
  readonly message: string;
  readonly traceId?: string;
  readonly fieldErrors: Readonly<Record<string, readonly string[]>>;
}
