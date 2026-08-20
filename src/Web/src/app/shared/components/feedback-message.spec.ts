import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { FeedbackMessage } from './feedback-message';

describe('FeedbackMessage', () => {
  it('should allow the user to dismiss a message', () => {
    const fixture = TestBed.createComponent(FeedbackMessage);
    fixture.componentRef.setInput('title', 'Operação concluída.');
    fixture.detectChanges();

    (fixture.nativeElement.querySelector('button') as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Operação concluída.');
  });

  it('should automatically dismiss success messages', () => {
    vi.useFakeTimers();
    const fixture = TestBed.createComponent(FeedbackMessage);
    fixture.componentRef.setInput('kind', 'success');
    fixture.componentRef.setInput('title', 'Produto cadastrado com sucesso.');
    fixture.detectChanges();

    vi.advanceTimersByTime(5_000);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('Produto cadastrado com sucesso.');
    fixture.destroy();
    vi.useRealTimers();
  });
});
