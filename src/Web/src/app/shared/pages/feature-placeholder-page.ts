import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-feature-placeholder-page',
  templateUrl: './feature-placeholder-page.html',
})
export class FeaturePlaceholderPage {
  readonly eyebrow = input.required<string>();
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly actionLabel = input('');
  readonly actionLink = input('');
}
