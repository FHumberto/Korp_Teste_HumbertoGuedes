import { Component, input } from '@angular/core';

@Component({
  selector: 'app-processing-button',
  template: `
    <button type="submit" [disabled]="disabled() || processing()" class="inline-flex min-h-10 items-center justify-center gap-2 rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700 disabled:cursor-not-allowed disabled:opacity-60">
      @if (processing()) { <span class="size-4 animate-spin rounded-full border-2 border-blue-200 border-t-white" aria-hidden="true"></span> }
      {{ processing() ? processingLabel() : label() }}
    </button>
  `,
})
export class ProcessingButton {
  readonly label = input.required<string>();
  readonly processingLabel = input('Processando...');
  readonly processing = input(false);
  readonly disabled = input(false);
}
