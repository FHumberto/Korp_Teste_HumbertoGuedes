import { Component, input } from '@angular/core';

@Component({
  selector: 'app-processing-button',
  templateUrl: './processing-button.html',
})
export class ProcessingButton {
  readonly label = input.required<string>();
  readonly processingLabel = input('Processando...');
  readonly processing = input(false);
  readonly disabled = input(false);
  readonly icon = input<'none' | 'print' | 'document'>('none');
}
