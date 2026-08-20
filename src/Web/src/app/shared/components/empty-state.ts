import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  templateUrl: './empty-state.html',
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly description = input('');
  readonly actionLabel = input('');
  readonly action = output<void>();
}
