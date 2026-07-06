import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { SnackbarComponent } from './components/snackbar.component';
import { ReadingSnapshot } from './services/statistics.service';
import { SignalRConnectionState, SignalRService } from './services/signalr.service';
import { SnackbarService } from './services/snackbar.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, SnackbarComponent],
  template: `
    <header class="toolbar">
      <span class="brand">Data Statistics</span>
      <span class="datetime">{{ now | date:'medium' }}</span>
      <span class="live-status" [class.live]="connectionState === 'connected'">
        {{ connectionState === 'connected' ? 'Live' : 'Offline' }}
      </span>
      <nav>
        <a routerLink="/">Dashboard</a>
        <a routerLink="/charts">Charts</a>
        <a routerLink="/aggregations">Aggregations</a>
      </nav>
    </header>
    <main>
      <router-outlet />
    </main>
    <footer class="footer">
      <p class="footer-message">Created with pleasure.</p>
    </footer>
    <app-snackbar />
  `,
  styles: [`
    :host { display: flex; flex-direction: column; min-height: 100vh; }
    .toolbar { display: flex; align-items: center; gap: 1rem; padding: 1rem 1.5rem; background: #3f51b5; color: #fff; }
    .datetime { font-size: 0.875rem; opacity: 0.9; }
    .brand { font-weight: 600; margin-right: auto; }
    .live-status { font-size: 0.75rem; opacity: 0.7; }
    .live-status.live { opacity: 1; color: #a5d6a7; }
    nav { display: flex; gap: 1rem; }
    a { color: #fff; text-decoration: none; }
    main { padding: 1.5rem; max-width: 1200px; margin: 0 auto; width: 100%; box-sizing: border-box; flex: 1; }
    .footer { background: #3f51b5; color: #fff; padding: 3rem 1.5rem; text-align: center; }
    .footer-message { margin: 0; font-size: 4rem; font-weight: 800; line-height: 1.2; text-transform: uppercase; letter-spacing: 0.05em; }
    @media (max-width: 768px) { .footer-message { font-size: 2rem; } }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly signalR = inject(SignalRService);
  private readonly snackbar = inject(SnackbarService);
  private subscription?: Subscription;
  private stateSubscription?: Subscription;
  private clockTimer?: ReturnType<typeof setInterval>;

  now = new Date();
  connectionState: SignalRConnectionState = 'disconnected';

  ngOnInit(): void {
    this.clockTimer = setInterval(() => {
      this.now = new Date();
    }, 1000);

    this.subscription = this.signalR.readingUpdated$.subscribe((update) => {
      this.snackbar.show(this.formatUpdateMessage(update));
    });

    this.stateSubscription = this.signalR.connectionState$.subscribe((state) => {
      this.connectionState = state;
    });

    void this.connectLiveUpdates();
  }

  ngOnDestroy(): void {
    if (this.clockTimer) {
      clearInterval(this.clockTimer);
    }
    this.subscription?.unsubscribe();
    this.stateSubscription?.unsubscribe();
    void this.signalR.stop();
  }

  private async connectLiveUpdates(): Promise<void> {
    try {
      await this.signalR.start();
    } catch {
      this.snackbar.show(
        'Cannot connect to live updates (SignalR). Check notification service on port 5300.',
        10000
      );
    }
  }

  private formatUpdateMessage(update: ReadingSnapshot): string {
    let value: string;
    if (update.numericValue != null) {
      value = String(update.numericValue);
    } else if (update.boolValue != null) {
      value = update.boolValue ? 'Detected' : 'Clear';
    } else {
      value = '—';
    }

    return `Reading updated: ${update.type} / ${update.name} — ${value}`;
  }
}

