import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { SnackbarService } from '../services/snackbar.service';

@Component({
  selector: 'app-snackbar',
  standalone: true,
  template: `
    @if (message) {
      <div class="snackbar" role="status">{{ message }}</div>
    }
  `,
  styles: [`
    .snackbar {
      position: fixed;
      bottom: 1.5rem;
      left: 50%;
      transform: translateX(-50%);
      background: #323232;
      color: #fff;
      padding: 0.75rem 1.25rem;
      border-radius: 4px;
      box-shadow: 0 3px 8px rgba(0, 0, 0, 0.3);
      font-size: 0.875rem;
      z-index: 1000;
      animation: slideUp 0.25s ease-out;
      max-width: min(90vw, 480px);
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translateX(-50%) translateY(1rem);
      }
      to {
        opacity: 1;
        transform: translateX(-50%) translateY(0);
      }
    }
  `]
})
export class SnackbarComponent implements OnInit, OnDestroy {
  private readonly snackbar = inject(SnackbarService);
  private readonly cdr = inject(ChangeDetectorRef);
  private subscription?: Subscription;

  message = '';

  ngOnInit(): void {
    this.subscription = this.snackbar.message$.subscribe((message) => {
      this.message = message;
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
