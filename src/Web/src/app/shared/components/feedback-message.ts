import { Component, OnDestroy, OnInit, input, signal } from '@angular/core';

export type FeedbackKind = 'error' | 'success' | 'info';

@Component({
  selector: 'app-feedback-message',
  host: { class: 'block' },
  templateUrl: './feedback-message.html',
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
