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
          <div class="spinner"></div>
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
      background: rgba(0,0,0,0.4);
      backdrop-filter: blur(2px);
      z-index: 9998; display: flex;
      align-items: center; justify-content: center;
    }
    .loading-card {
      background: white; border-radius: 20px;
      padding: 32px 40px; display: flex;
      flex-direction: column; align-items: center;
      gap: 16px; box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    }
    .spinner {
      width: 48px; height: 48px; border-radius: 50%;
      border: 4px solid #FEE2E2;
      border-top-color: #FF6B35;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin {
      to { transform: rotate(360deg); }
    }
    .loading-text {
      font-size: 14px; color: #6B7280;
      font-weight: 500;
    }
  `]
})
export class LoadingComponent {
  loadingSvc = inject(LoadingService);
}