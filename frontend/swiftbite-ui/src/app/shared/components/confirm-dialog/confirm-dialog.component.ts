import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from '../../../core/services/confirm.service';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (confirmSvc.request(); as req) {
      <div class="confirm-overlay" (click)="confirmSvc.resolve(false)">
        <div class="confirm-card" [class.is-danger]="req.danger" (click)="$event.stopPropagation()">
          <h2 class="confirm-title">{{ req.title }}</h2>
          <p class="confirm-message">{{ req.message }}</p>
          <div class="confirm-actions">
            <button class="btn btn-ghost" (click)="confirmSvc.resolve(false)">
              {{ req.cancelLabel || 'Cancel' }}
            </button>
            <button
              class="btn"
              [class.btn-danger]="req.danger"
              [class.btn-primary]="!req.danger"
              (click)="confirmSvc.resolve(true)">
              {{ req.confirmLabel || 'Confirm' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .confirm-overlay {
      position: fixed; inset: 0;
      background: var(--color-overlay);
      z-index: 10000;
      display: flex; align-items: center; justify-content: center;
      padding: 1rem;
      animation: fadeIn 0.15s ease;
    }
    .confirm-card {
      background: var(--color-surface-raised);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      padding: 1.5rem;
      max-width: 380px;
      width: 100%;
      animation: popIn 0.18s cubic-bezier(0.2, 0.8, 0.3, 1.1);
      border-top: 3px solid var(--color-primary);
    }
    .confirm-card.is-danger { border-top-color: var(--color-danger); }
    .confirm-title {
      font-size: var(--text-lg);
      font-weight: 700;
      color: var(--color-text);
      margin: 0 0 0.5rem;
    }
    .confirm-message {
      font-size: var(--text-sm);
      color: var(--color-text-muted);
      margin: 0 0 1.25rem;
      line-height: 1.5;
    }
    .confirm-actions {
      display: flex; justify-content: flex-end; gap: 0.6rem;
    }
    .btn {
      font-size: var(--text-sm);
      font-weight: 600;
      padding: 0.5rem 1rem;
      border-radius: var(--radius-md);
      border: 1px solid transparent;
      cursor: pointer;
      transition: filter 0.12s ease, transform 0.12s ease;
    }
    .btn:active { transform: scale(0.97); }
    .btn-ghost {
      background: transparent;
      color: var(--color-text-muted);
      border-color: var(--color-border);
    }
    .btn-ghost:hover { background: var(--color-paper); }
    .btn-primary { background: var(--color-primary); color: #fff; }
    .btn-primary:hover { background: var(--color-primary-hover); }
    .btn-danger { background: var(--color-danger); color: #fff; }
    .btn-danger:hover { filter: brightness(1.08); }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    @keyframes popIn { from { opacity: 0; transform: scale(0.94) translateY(6px); } to { opacity: 1; transform: scale(1) translateY(0); } }
    @media (prefers-reduced-motion: reduce) {
      .confirm-overlay, .confirm-card { animation: none; }
    }
  `],
})
export class ConfirmDialogComponent {
  confirmSvc = inject(ConfirmService);
}
