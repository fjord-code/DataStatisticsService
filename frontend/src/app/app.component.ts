import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { SnackbarComponent } from './components/snackbar.component';
import { ReadingSnapshot } from './services/statistics.service';
import { SignalRService } from './services/signalr.service';
import { SnackbarService } from './services/snackbar.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, SnackbarComponent],
  template: `
    <header class="toolbar">
      <span class="brand">Data Statistics</span>
      <span class="datetime">{{ now | date:'medium' }}</span>
      <nav>
        <a routerLink="/">Dashboard</a>
        <a routerLink="/charts">Charts</a>
        <a routerLink="/aggregations">Aggregations</a>
      </nav>
    </header>
    <main>
      <router-outlet />
    </main>
    <app-snackbar />
  `,
  styles: [`
    .toolbar { display: flex; align-items: center; gap: 1rem; padding: 1rem 1.5rem; background: #3f51b5; color: #fff; }
    .datetime { font-size: 0.875rem; opacity: 0.9; }
    .brand { font-weight: 600; margin-right: auto; }
    nav { display: flex; gap: 1rem; }
    a { color: #fff; text-decoration: none; }
    main { padding: 1.5rem; max-width: 1200px; margin: 0 auto; }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly signalR = inject(SignalRService);
  private readonly snackbar = inject(SnackbarService);
  private subscription?: Subscription;
  private clockTimer?: ReturnType<typeof setInterval>;

  now = new Date();

  ngOnInit(): void {
    this.clockTimer = setInterval(() => {
      this.now = new Date();
    }, 1000);

    void this.signalR.start();
    this.subscription = this.signalR.readingUpdated$.subscribe((update) => {
      this.snackbar.show(this.formatUpdateMessage(update));
    });
  }

  ngOnDestroy(): void {
    if (this.clockTimer) {
      clearInterval(this.clockTimer);
    }
    this.subscription?.unsubscribe();
    void this.signalR.stop();
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
