import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';import { ToastService } from '../../../../core/services/toast.service';


@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      @for (toast of toastSvc.toasts(); track toast.id) {
        <div class="toast"
          [class]="'toast-' + toast.type"
          (click)="toastSvc.remove(toast.id)">
          <span class="toast-icon">
            {{ icons[toast.type] }}
          </span>
          <span class="toast-message">
            {{ toast.message }}
          </span>
          <button class="toast-close">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; top: 5rem; right: 1.25rem;
      z-index: 9999; display: flex;
      flex-direction: column; gap: 0.6rem;
      max-width: 380px; pointer-events: none;
    }
    .toast {
      display: flex; align-items: center; gap: 0.6rem;
      padding: 0.85rem 1rem; border-radius: var(--radius-md);
      box-shadow: var(--shadow-md);
      cursor: pointer; pointer-events: all;
      animation: slideIn 0.25s cubic-bezier(0.2, 0.8, 0.3, 1.1);
      font-size: var(--text-sm); font-weight: 500;
      border: 1px solid transparent;
    }
    @keyframes slideIn {
      from { transform: translateX(110%); opacity: 0; }
      to   { transform: translateX(0);    opacity: 1; }
    }
    .toast-success {
      background: var(--color-accent-soft); color: var(--color-accent);
      border-color: var(--color-accent);
    }
    .toast-error {
      background: var(--color-danger-soft); color: var(--color-danger);
      border-color: var(--color-danger);
    }
    .toast-warning {
      background: var(--color-warning-soft); color: var(--color-warning);
      border-color: var(--color-warning);
    }
    .toast-info {
      background: var(--color-info-soft); color: var(--color-info);
      border-color: var(--color-info);
    }
    .toast-icon { font-size: var(--text-lg); flex-shrink: 0; }
    .toast-message { flex: 1; line-height: 1.4; }
    .toast-close {
      background: none; border: none; font-size: var(--text-lg);
      color: currentColor;
      cursor: pointer; opacity: 0.5; padding: 0;
      line-height: 1;
      &:hover { opacity: 1; }
    }
    @media (prefers-reduced-motion: reduce) {
      .toast { animation: none; }
    }
    @media (max-width: 480px) {
      .toast-container { left: 1rem; right: 1rem; top: 4.5rem; max-width: none; }
    }
  `]
})
export class ToastComponent {
  toastSvc = inject(ToastService);
  icons: Record<string, string> = {
    success: '✅', error: '❌',
    warning: '⚠️', info: 'ℹ️'
  };
}