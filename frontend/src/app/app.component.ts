import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="toolbar">
      <span class="brand">Data Statistics</span>
      <nav>
        <a routerLink="/">Dashboard</a>
        <a routerLink="/charts">Charts</a>
        <a routerLink="/aggregations">Aggregations</a>
      </nav>
    </header>
    <main>
      <router-outlet />
    </main>
  `,
  styles: [`
    .toolbar { display: flex; align-items: center; gap: 1rem; padding: 1rem 1.5rem; background: #3f51b5; color: #fff; }
    .brand { font-weight: 600; margin-right: auto; }
    nav { display: flex; gap: 1rem; }
    a { color: #fff; text-decoration: none; }
    main { padding: 1.5rem; max-width: 1200px; margin: 0 auto; }
  `]
})
export class AppComponent {}
