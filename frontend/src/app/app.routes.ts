import { Routes } from '@angular/router';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ChartsComponent } from './pages/charts/charts.component';
import { AggregationsComponent } from './pages/aggregations/aggregations.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'charts', component: ChartsComponent },
  { path: 'aggregations', component: AggregationsComponent }
];
