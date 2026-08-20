import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `<div class="rounded-xl border border-dashed border-slate-300 bg-white p-8 text-center"><p class="font-semibold text-slate-900">{{ title() }}</p><p class="mt-1 text-sm text-slate-600">{{ description() }}</p></div>`,
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly description = input('');
}
