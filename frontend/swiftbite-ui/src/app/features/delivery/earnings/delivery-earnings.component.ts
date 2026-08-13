// ── earnings.component.ts ─────────────────────────────────────
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryService, EarningsData } from '../../../core/services/delivery.service';

export { EarningsComponent };

@Component({
  selector: 'app-delivery-earnings',
  standalone: true,
  imports: [CommonModule],
 templateUrl: './delivery-earnings.component.html',
  styleUrls: ['./delivery-earnings.component.scss'],
})
class EarningsComponent implements OnInit {
  private svc = inject(DeliveryService);
  loading = signal(true);
  error   = signal<string | null>(null);
  data    = signal<EarningsData | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.svc.getEarnings().subscribe({
      next:  d => { this.data.set(d); this.loading.set(false); },
      error: () => {
        this.loading.set(false);
        this.error.set('Could not load your earnings. Please try again.');
      },
    });
  }
}