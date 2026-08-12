import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from '../../../../core/services/loading.service';


@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loadingSvc.isLoading()) {
      <div class="loading-overlay">
        <div class="loading-card">
          <div class="loading-dots">
            <span></span><span></span><span></span>
          </div>
          <p class="loading-text">
            {{ loadingSvc.message() }}
          </p>
        </div>
      </div>
    }
  `,
  styles: [`
    .loading-overlay {
      position: fixed; inset: 0;
      background: var(--color-overlay);
      backdrop-filter: blur(2px);
      z-index: 9998; display: flex;
      align-items: center; justify-content: center;
      animation: fadeIn 0.15s ease;
    }
    .loading-card {
      background: var(--color-surface-raised);
      border-radius: var(--radius-lg);
      padding: 1.75rem 2.25rem; display: flex;
      flex-direction: column; align-items: center;
      gap: 0.9rem; box-shadow: var(--shadow-lg);
    }
    .loading-dots {
      display: flex; gap: 0.4rem;
    }
    .loading-dots span {
      width: 0.6rem; height: 0.6rem; border-radius: 50%;
      background: var(--color-primary);
      animation: bounce 0.9s ease-in-out infinite;
    }
    .loading-dots span:nth-child(2) { animation-delay: 0.15s; }
    .loading-dots span:nth-child(3) { animation-delay: 0.3s; }
    @keyframes bounce {
      0%, 80%, 100% { transform: scale(0.6); opacity: 0.5; }
      40% { transform: scale(1); opacity: 1; }
    }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
    .loading-text {
      font-size: var(--text-sm); color: var(--color-text-muted);
      font-weight: 500;
    }
    @media (prefers-reduced-motion: reduce) {
      .loading-overlay { animation: none; }
      .loading-dots span { animation: none; opacity: 1; transform: scale(0.85); }
    }
  `]
})
export class LoadingComponent {
  loadingSvc = inject(LoadingService);
}