import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Input, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, finalize, map, of, scan, takeWhile, timer, withLatestFrom } from 'rxjs';

@Component({
  selector: 'app-countdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './countdown.component.html',
})
export class CountdownComponent {

  private readonly destroyRef = inject(DestroyRef);

  @Input() isRunning$: Observable<boolean> = of(true);
  @Input() durationInSeconds = 5;
  @Input() message = '';
  @Input() title?: string;

  @Output() completed = new EventEmitter();

  readonly countdown$ = timer(0, 1000).pipe(
    withLatestFrom(this.isRunning$),
    map(([_ , isRunning]) => isRunning ? 1 : 0),
    scan((remainingSeconds, decrement) => remainingSeconds - decrement, this.durationInSeconds + 1),
    takeWhile(x => x > 0),
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.completed.emit()),
  );
}
