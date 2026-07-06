import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatisticsService, TypeAggregation, LocationAggregation } from '../../services/statistics.service';

@Component({
  selector: 'app-aggregations',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Aggregations</h2>
    @if (loading) {
      <p class="muted">Loading...</p>
    } @else if (error) {
      <p class="error">{{ error }}</p>
    } @else {
      <div class="columns">
        <article class="card">
          <h3>By type</h3>
          @for (item of byType; track item.type) {
            <p>{{ item.type }}: {{ item.count }} readings (avg {{ item.avgNumeric ?? '—' }})</p>
          }
        </article>
        <article class="card">
          <h3>By location</h3>
          @for (item of byLocation; track item.name) {
            <p>{{ item.name }}: {{ item.count }} readings (avg {{ item.avgNumeric ?? '—' }})</p>
          }
        </article>
      </div>
    }
  `,
  styles: [`
    .columns { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; }
    .card { background: #fff; border-radius: 8px; padding: 1rem; box-shadow: 0 1px 4px rgba(0,0,0,.12); }
    .error { color: #c62828; }
    .muted { color: #666; }
  `]
})
export class AggregationsComponent implements OnInit {
  private readonly statistics = inject(StatisticsService);

  byType: TypeAggregation[] = [];
  byLocation: LocationAggregation[] = [];
  loading = true;
  error = '';

  ngOnInit(): void {
    this.statistics.getAggregationsByType().subscribe({
      next: (data) => (this.byType = data),
      error: () => (this.error = 'Failed to load aggregations.')
    });
    this.statistics.getAggregationsByLocation().subscribe({
      next: (data) => {
        this.byLocation = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load aggregations.';
        this.loading = false;
      }
    });
  }
}
