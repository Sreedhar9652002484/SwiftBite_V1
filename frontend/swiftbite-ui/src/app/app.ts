import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './features/shared/components/toast/toast.component';
import { LoadingComponent } from './features/shared/components/loading/loading.component';
import { NetworkStatusComponent } from './features/shared/components/network/network-status.component';
import { ConfirmDialogComponent } from './shared/components/confirm-dialog/confirm-dialog.component';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastComponent, LoadingComponent, NetworkStatusComponent, ConfirmDialogComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('swiftbite-ui');
  // Injected so the theme class is applied to <html> as early as possible.
  private theme = inject(ThemeService);
}
