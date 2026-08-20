import { Component, input } from '@angular/core';

export type FeedbackKind = 'error' | 'success' | 'info';

@Component({
  selector: 'app-feedback-message',
  template: `
    <div
      class="rounded-lg border p-4 text-sm"
      [class]="classes()"
      [attr.role]="kind() === 'error' ? 'alert' : 'status'"
      aria-live="polite"
    >
      <p class="font-semibold">{{ title() }}</p>
      @if (message()) { <p class="mt-1">{{ message() }}</p> }
      @if (traceId()) { <p class="mt-2 text-xs opacity-80">Referência: {{ traceId() }}</p> }
    </div>
  `,
})
export class FeedbackMessage {
  readonly kind = input<FeedbackKind>('info');
  readonly title = input.required<string>();
  readonly message = input('');
  readonly traceId = input<string>();

  protected classes(): string {
    if (this.kind() === 'error') return 'border-red-300 bg-red-50 text-red-900';
    if (this.kind() === 'success') return 'border-emerald-300 bg-emerald-50 text-emerald-900';
    return 'border-blue-300 bg-blue-50 text-blue-900';
  }
}
