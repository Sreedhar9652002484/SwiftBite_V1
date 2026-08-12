import { Component, inject } from '@angular/core';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  template: `
    <button
      type="button"
      class="theme-toggle"
      [class.is-dark]="theme.isDark()"
      (click)="theme.toggle()"
      [attr.aria-label]="theme.isDark() ? 'Switch to light mode' : 'Switch to dark mode'"
      [attr.aria-pressed]="theme.isDark()">
      <span class="theme-toggle__icon">{{ theme.isDark() ? '🌙' : '☀️' }}</span>
    </button>
  `,
  styles: [`
    .theme-toggle {
      width: 2.25rem;
      height: 2.25rem;
      border-radius: var(--radius-md);
      border: 1px solid var(--color-border);
      background: var(--color-surface);
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: background 0.15s ease, border-color 0.15s ease, transform 0.15s ease;
    }
    .theme-toggle:hover { border-color: var(--color-primary); }
    .theme-toggle:active { transform: scale(0.92); }
    .theme-toggle:focus-visible {
      outline: 2px solid var(--color-primary);
      outline-offset: 2px;
    }
    .theme-toggle__icon {
      font-size: var(--text-md);
      line-height: 1;
    }
    @media (prefers-reduced-motion: reduce) {
      .theme-toggle { transition: none; }
    }
  `],
})
export class ThemeToggleComponent {
  theme = inject(ThemeService);
}
