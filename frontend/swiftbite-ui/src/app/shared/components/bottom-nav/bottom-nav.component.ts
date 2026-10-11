import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-bottom-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <nav class="bnav" aria-label="Primary">
      @for (t of tabs; track t.path) {
        <a [routerLink]="t.path" routerLinkActive="is-active" class="bnav__tab">
          <span class="bnav__icon" aria-hidden="true">{{ t.icon }}</span>
          <span class="bnav__label">{{ t.label }}</span>
        </a>
      }
    </nav>
  `,
  styles: [`
    .bnav {
      position: fixed; left: 0; right: 0; bottom: 0; z-index: 60;
      display: flex; justify-content: space-around;
      padding: 0.35rem 0.5rem calc(0.35rem + env(safe-area-inset-bottom));
      background: color-mix(in srgb, var(--color-surface) 88%, transparent);
      backdrop-filter: blur(14px);
      border-top: 1px solid var(--color-border);
    }
    .bnav__tab {
      flex: 1; display: flex; flex-direction: column; align-items: center; gap: 0.1rem;
      padding: 0.35rem 0; border-radius: var(--radius-lg);
      text-decoration: none; color: var(--color-text-muted);
      font-size: var(--text-xs); font-weight: 700; transition: color 0.15s ease;
    }
    .bnav__icon { font-size: 1.3rem; filter: grayscale(1); opacity: 0.7; transition: transform 0.15s ease; }
    .bnav__tab.is-active { color: var(--color-primary); }
    .bnav__tab.is-active .bnav__icon { filter: none; opacity: 1; transform: translateY(-2px) scale(1.1); }
    @media (min-width: 1024px) { .bnav { display: none; } }
  `]
})
export class BottomNavComponent {
  tabs = [
    { path: '/home',     icon: '🏠', label: 'Home' },
    { path: '/orders',   icon: '🧾', label: 'Orders' },
    { path: '/become-partner', icon: '🤝', label: 'Partner' },
    { path: '/profile',  icon: '👤', label: 'Profile' },
  ];
}
