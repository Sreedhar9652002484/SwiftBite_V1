import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-gray-50 flex items-center justify-center">
      <div class="text-center">
        <div class="text-6xl mb-4">🚫</div>
        <h1 class="text-2xl font-bold text-gray-800">Access Denied</h1>
        <p class="text-gray-500 mt-2">
          You don't have permission to view this page.
        </p>
        <button (click)="router.navigate(['/home'])"
                class="mt-6 px-6 py-3 bg-orange-500 text-white
                       font-bold rounded-xl hover:bg-orange-600
                       transition-colors">
          Go Home
        </button>
      </div>
    </div>
  `
})
export class UnauthorizedComponent {
  router = inject(Router);
}
