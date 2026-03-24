import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error'
  | 'warning' | 'info';

export interface Toast {
  id:       number;
  message:  string;
  type:     ToastType;
  duration: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts = signal<Toast[]>([]);
  private counter = 0;

  success(message: string, duration = 3000): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration = 5000): void {
    this.show(message, 'error', duration);
  }

  warning(message: string, duration = 4000): void {
    this.show(message, 'warning', duration);
  }

  info(message: string, duration = 3000): void {
    this.show(message, 'info', duration);
  }

  private show(message: string,
    type: ToastType, duration: number): void {
    const id = ++this.counter;
    this.toasts.update(t => [...t, {
      id, message, type, duration
    }]);
    setTimeout(() => this.remove(id), duration);
  }

  remove(id: number): void {
    this.toasts.update(t =>
      t.filter(toast => toast.id !== id));
  }
}