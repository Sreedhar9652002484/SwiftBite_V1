import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private _count  = 0;
  isLoading = signal(false);
  message   = signal('');

  show(message = 'Loading...'): void {
    this._count++;
    this.isLoading.set(true);
    this.message.set(message);
  }

  hide(): void {
    this._count = Math.max(0, this._count - 1);
    if (this._count === 0) {
      this.isLoading.set(false);
      this.message.set('');
    }
  }

  forceHide(): void {
    this._count = 0;
    this.isLoading.set(false);
  }
}