import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `<div class="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center"><svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" class="mx-auto size-10 text-slate-400"><path d="M6 3h9l3 3v15H6zM9 11h6M9 15h6" stroke-linecap="round" stroke-linejoin="round" /></svg><p class="mt-3 font-semibold text-slate-900">{{ title() }}</p><p class="mt-1 text-sm text-slate-600">{{ description() }}</p>@if (actionLabel()) { <button type="button" (click)="action.emit()" class="mt-5 inline-flex min-h-10 items-center gap-2 rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-800 active:bg-blue-900"><span aria-hidden="true">+</span>{{ actionLabel() }}</button> }</div>`,
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly description = input('');
  readonly actionLabel = input('');
  readonly action = output<void>();
}
