import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-feature-placeholder-page',
  template: `<section><div class="flex flex-wrap items-start justify-between gap-4"><div><p class="text-sm font-semibold text-blue-700">{{ eyebrow() }}</p><h1 class="mt-1 text-3xl font-bold tracking-tight">{{ title() }}</h1><p class="mt-2 max-w-2xl text-slate-600">{{ description() }}</p></div>@if (actionLabel() && actionLink()) { <a [routerLink]="actionLink()" class="rounded-lg bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-blue-700">{{ actionLabel() }}</a> }</div><div class="mt-8 rounded-xl border border-slate-200 bg-white p-6 shadow-sm"><p class="text-sm text-slate-600">A estrutura desta página está pronta para a implementação do fluxo funcional.</p></div></section>`,
})
export class FeaturePlaceholderPage {
  readonly eyebrow = input.required<string>();
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly actionLabel = input('');
  readonly actionLink = input('');
}
