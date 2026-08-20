import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-indicator',
  template: `
    <div class="flex items-center gap-3 text-sm text-slate-700" role="status" aria-live="polite">
      <span class="size-5 animate-spin rounded-full border-2 border-slate-300 border-t-blue-700" aria-hidden="true"></span>
      <span>{{ label() }}</span>
    </div>
  `,
})
export class LoadingIndicator {
  readonly label = input('Carregando...');
}
