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
      position: fixed; top: 80px; right: 20px;
      z-index: 9999; display: flex;
      flex-direction: column; gap: 10px;
      max-width: 380px; pointer-events: none;
    }
    .toast {
      display: flex; align-items: center; gap: 10px;
      padding: 14px 16px; border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.15);
      cursor: pointer; pointer-events: all;
      animation: slideIn 0.3s ease;
      font-size: 14px; font-weight: 500;
    }
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to   { transform: translateX(0);    opacity: 1; }
    }
    .toast-success {
      background: #ECFDF5; color: #065F46;
      border: 1px solid #A7F3D0;
    }
    .toast-error {
      background: #FEF2F2; color: #991B1B;
      border: 1px solid #FECACA;
    }
    .toast-warning {
      background: #FFFBEB; color: #92400E;
      border: 1px solid #FDE68A;
    }
    .toast-info {
      background: #EFF6FF; color: #1E40AF;
      border: 1px solid #BFDBFE;
    }
    .toast-icon { font-size: 18px; flex-shrink: 0; }
    .toast-message { flex: 1; line-height: 1.4; }
    .toast-close {
      background: none; border: none; font-size: 18px;
      cursor: pointer; opacity: 0.5; padding: 0;
      line-height: 1;
      &:hover { opacity: 1; }
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