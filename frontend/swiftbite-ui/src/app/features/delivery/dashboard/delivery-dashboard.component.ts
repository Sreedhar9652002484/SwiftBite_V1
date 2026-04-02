import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { DeliveryService, DeliveryJob, DeliveryPartner } from '../../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './delivery-dashboard.component.html',
  styleUrls: ['./delivery-dashboard.component.scss'],
})
export class DeliveryDashboardComponent implements OnInit {

  auth        = inject(AuthService);
  deliverySvc = inject(DeliveryService);

  loadingProfile = signal(true);
  loadingJobs    = signal(true);

  profile    = signal<DeliveryPartner | null>(null);
  allJobs    = signal<DeliveryJob[]>([]);
  activeJobs = signal<DeliveryJob[]>([]);

  // ── Computed stats ─────────────────────────────────────
  todayEarnings = computed(() => {
    const today = new Date();
    return this.allJobs()
      .filter(j =>
        j.status === 'Delivered' &&
        j.deliveredAt &&
        this.isToday(j.deliveredAt)
      )
      .reduce((s, j) => s + j.deliveryFee, 0);
  });

  todayDeliveries = computed(() =>
    this.allJobs().filter(j =>
      j.status === 'Delivered' &&
      j.deliveredAt &&
      this.isToday(j.deliveredAt)
    ).length
  );

  pendingJobs = computed(() =>
    this.allJobs().filter(j => j.status === 'Assigned').length
  );

  recentJobs = computed(() =>
    [...this.allJobs()]
      .sort((a, b) => new Date(b.assignedAt).getTime() - new Date(a.assignedAt).getTime())
      .slice(0, 5)
  );

  loading = computed(() => this.loadingProfile() || this.loadingJobs());

  ngOnInit(): void {
    this.loadProfile();
    this.loadJobs();
  }

  private loadProfile(): void {
    this.deliverySvc.getProfile().subscribe({
      next:  p => { this.profile.set(p); this.loadingProfile.set(false); },
      error: () => this.loadingProfile.set(false),
    });
  }

  private loadJobs(): void {
    this.deliverySvc.getJobs().subscribe({
      next: jobs => {
        this.allJobs.set(jobs);
        this.activeJobs.set(jobs.filter(j =>
          ['Assigned','Accepted','PickedUp'].includes(j.status)
        ));
        this.loadingJobs.set(false);
      },
      error: () => this.loadingJobs.set(false),
    });
  }

  statusClass(status: string): string {
    const map: Record<string, string> = {
      Assigned:  'badge-assigned',
      Accepted:  'badge-accepted',
      PickedUp:  'badge-pickedup',
      Delivered: 'badge-delivered',
      Rejected:  'badge-rejected',
      Cancelled: 'badge-cancelled',
    };
    return map[status] ?? 'badge-assigned';
  }

  timeAgo(dateStr: string): string {
    const mins = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    if (mins < 1440) return `${Math.floor(mins/60)}h ago`;
    return `${Math.floor(mins/1440)}d ago`;
  }

  private isToday(dateStr: string): boolean {
    const d = new Date(dateStr), n = new Date();
    return d.getDate() === n.getDate() &&
           d.getMonth() === n.getMonth() &&
           d.getFullYear() === n.getFullYear();
  }
}