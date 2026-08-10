import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './features/shared/components/toast/toast.component';
import { LoadingComponent } from './features/shared/components/loading/loading.component';
import { NetworkStatusComponent } from './features/shared/components/network/network-status.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,ToastComponent,
    // LoadingComponent,
    NetworkStatusComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('swiftbite-ui');
}
