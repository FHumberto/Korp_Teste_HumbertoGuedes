import { Component, OnDestroy, OnInit, input, signal } from '@angular/core';

export type FeedbackKind = 'error' | 'success' | 'info';

@Component({
  selector: 'app-feedback-message',
  host: { class: 'block' },
  template: `
    @if (visible()) { <div
      class="rounded-lg border p-4 text-sm"
      [class]="classes()"
      [attr.role]="kind() === 'error' ? 'alert' : 'status'"
      aria-live="polite"
    >
      <div class="flex items-start justify-between gap-4">
        <div><p class="font-semibold">{{ title() }}</p>@if (message()) { <p class="mt-1">{{ message() }}</p> }@if (traceId()) { <p class="mt-2 text-xs opacity-80">Referência: {{ traceId() }}</p> }</div>
        <button type="button" (click)="dismiss()" class="-m-1 shrink-0 rounded-md p-1 text-lg leading-none opacity-70 hover:bg-black/5 hover:opacity-100" aria-label="Fechar mensagem">×</button>
      </div>
    </div> }
  `,
})
export class FeedbackMessage implements OnInit, OnDestroy {
  readonly kind = input<FeedbackKind>('info');
  readonly title = input.required<string>();
  readonly message = input('');
  readonly traceId = input<string>();
  protected readonly visible = signal(true);
  private dismissTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    if (this.kind() === 'success') this.dismissTimer = setTimeout(() => this.dismiss(), 5_000);
  }

  ngOnDestroy(): void { if (this.dismissTimer) clearTimeout(this.dismissTimer); }
  protected dismiss(): void { this.visible.set(false); }

  protected classes(): string {
    if (this.kind() === 'error') return 'border-red-300 bg-red-50 text-red-900';
    if (this.kind() === 'success') return 'border-emerald-300 bg-emerald-50 text-emerald-900';
    return 'border-blue-300 bg-blue-50 text-blue-900';
  }
}
