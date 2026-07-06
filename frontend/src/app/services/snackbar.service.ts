import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SnackbarService {
  readonly message$ = new Subject<string>();
  private dismissTimer?: ReturnType<typeof setTimeout>;

  show(message: string, durationMs = 4000): void {
    if (this.dismissTimer) {
      clearTimeout(this.dismissTimer);
    }

    this.message$.next(message);

    this.dismissTimer = setTimeout(() => {
      this.message$.next('');
      this.dismissTimer = undefined;
    }, durationMs);
  }
}
