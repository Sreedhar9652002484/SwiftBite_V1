import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PartnerService, PartnerApplication } from '../../../core/services/partner.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmService } from '../../../core/services/confirm.service';

@Component({
  selector: 'app-admin-partner-applications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-partner-applications.component.html',
  styleUrls: ['./admin-partner-applications.component.scss'],
})
export class AdminPartnerApplicationsComponent implements OnInit {
  private partnerSvc = inject(PartnerService);
  private toast = inject(ToastService);
  private confirmSvc = inject(ConfirmService);

  loading = signal(true);
  processing = signal<string | null>(null);
  applications = signal<PartnerApplication[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.partnerSvc.listPending().subscribe({
      next: (apps) => {
        this.applications.set(apps);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load applications.');
        this.loading.set(false);
      },
    });
  }

  approve(app: PartnerApplication): void {
    if (this.processing()) return;
    this.processing.set(app.id);
    this.partnerSvc.approve(app.id).subscribe({
      next: () => {
        this.toast.success(`${app.applicantName} approved as ${app.requestedRole}.`);
        this.applications.update((list) => list.filter((a) => a.id !== app.id));
        this.processing.set(null);
      },
      error: () => {
        this.toast.error('Failed to approve application.');
        this.processing.set(null);
      },
    });
  }

  async reject(app: PartnerApplication): Promise<void> {
    if (this.processing()) return;
    const ok = await this.confirmSvc.confirm({
      title: 'Reject application?',
      message: `Reject ${app.applicantName}'s partner application? This cannot be undone.`,
      confirmLabel: 'Reject',
      danger: true,
    });
    if (!ok) return;
    const note = prompt('Reason for rejection (optional):') ?? undefined;
    this.processing.set(app.id);
    this.partnerSvc.reject(app.id, note).subscribe({
      next: () => {
        this.toast.success(`Application rejected.`);
        this.applications.update((list) => list.filter((a) => a.id !== app.id));
        this.processing.set(null);
      },
      error: () => {
        this.toast.error('Failed to reject application.');
        this.processing.set(null);
      },
    });
  }

  roleLabel(role: string): string {
    return role === 'RestaurantAdmin' ? '🍽️ Restaurant Partner' : '🛵 Delivery Partner';
  }
}
