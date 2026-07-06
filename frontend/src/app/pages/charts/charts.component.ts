import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatisticsService, TimeBucket } from '../../services/statistics.service';

@Component({
  selector: 'app-charts',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Time series (last 24h)</h2>
    @if (loading) {
      <p class="muted">Loading...</p>
    } @else if (error) {
      <p class="error">{{ error }}</p>
    } @else {
      <article class="card">
        @for (bucket of buckets; track bucket.bucketStartUtc + bucket.name) {
          <div class="row">
            <span>{{ bucket.name }} / {{ bucket.type }}</span>
            <span>{{ bucket.bucketStartUtc | date: 'short' }}</span>
            <span>samples: {{ bucket.sampleCount }}</span>
            <span>avg: {{ bucket.avgNumeric ?? '—' }}</span>
          </div>
        }
        @if (buckets.length === 0) {
          <p>No bucket data yet.</p>
        }
      </article>
    }
  `,
  styles: [`
    .card { background: #fff; border-radius: 8px; padding: 1rem; box-shadow: 0 1px 4px rgba(0,0,0,.12); }
    .row { display: grid; grid-template-columns: 2fr 2fr 1fr 1fr; gap: 0.5rem; padding: 0.25rem 0; border-bottom: 1px solid #eee; }
    .error { color: #c62828; }
    .muted { color: #666; }
  `]
})
export class ChartsComponent implements OnInit {
  private readonly statistics = inject(StatisticsService);

  buckets: TimeBucket[] = [];
  loading = true;
  error = '';

  ngOnInit(): void {
    const to = new Date();
    const from = new Date(to.getTime() - 24 * 60 * 60 * 1000);
    this.statistics.getTimeBuckets(from.toISOString(), to.toISOString()).subscribe({
      next: (data) => {
        this.buckets = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load time series.';
        this.loading = false;
      }
    });
  }
}
