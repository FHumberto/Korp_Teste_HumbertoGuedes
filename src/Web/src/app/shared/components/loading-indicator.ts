import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-indicator',
  templateUrl: './loading-indicator.html',
})
export class LoadingIndicator {
  readonly label = input('Carregando...');
}
