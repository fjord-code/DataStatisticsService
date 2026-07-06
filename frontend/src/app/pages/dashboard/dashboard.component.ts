import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReadingSnapshot, StatisticsService } from '../../services/statistics.service';
import { SignalRService } from '../../services/signalr.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Latest readings</h2>
    @if (loading) {
      <p class="muted">Loading...</p>
    } @else if (error) {
      <p class="error">{{ error }}</p>
    } @else {
      <div class="grid">
        @for (reading of snapshots; track reading.lastEventId) {
          <article class="card">
            <h3>{{ reading.name }}</h3>
            <p class="subtitle">{{ reading.type }}</p>
            @if (reading.numericValue != null) {
              <p class="value">{{ reading.numericValue }}</p>
            } @else if (reading.boolValue != null) {
              <p class="value">{{ reading.boolValue ? 'Detected' : 'Clear' }}</p>
            } @else {
              <p class="value">—</p>
            }
            <small>{{ reading.updatedAtUtc | date: 'short' }}</small>
          </article>
        }
      </div>
    }
  `,
  styles: [`
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 1rem; }
    .card { background: #fff; border-radius: 8px; padding: 1rem; box-shadow: 0 1px 4px rgba(0,0,0,.12); }
    .subtitle { color: #666; margin-top: 0; }
    .value { font-size: 1.5rem; font-weight: 600; margin: 0.5rem 0; }
    .error { color: #c62828; }
    .muted { color: #666; }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly statistics = inject(StatisticsService);
  private readonly signalR = inject(SignalRService);
  private subscription?: Subscription;

  snapshots: ReadingSnapshot[] = [];
  loading = true;
  error = '';

  ngOnInit(): void {
    this.load();
    this.subscription = this.signalR.readingUpdated$.subscribe((update) => {
      const index = this.snapshots.findIndex((s) => s.type === update.type && s.name === update.name);
      if (index >= 0) {
        this.snapshots[index] = { ...this.snapshots[index], ...update };
      } else {
        this.snapshots = [update, ...this.snapshots];
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  private load(): void {
    this.statistics.getLatestSnapshots().subscribe({
      next: (data) => {
        this.snapshots = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load latest readings.';
        this.loading = false;
      }
    });
  }
}
