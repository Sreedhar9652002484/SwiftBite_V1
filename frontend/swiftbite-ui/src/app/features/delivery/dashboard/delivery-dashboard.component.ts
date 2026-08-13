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

  error = signal<string | null>(null);

  profile      = signal<DeliveryPartner | null>(null);
  // Partner's own job history (assigned/accepted/pickedup/delivered/rejected/cancelled) —
  // used for today's-earnings/deliveries and recent activity.
  myJobs       = signal<DeliveryJob[]>([]);
  // Partner's currently in-flight jobs (from the correct partner-scoped endpoint).
  activeJobs   = signal<DeliveryJob[]>([]);
  // Open marketplace pool of unassigned jobs available to claim.
  availableJobs = signal<DeliveryJob[]>([]);

  // ── Computed stats ─────────────────────────────────────
  todayEarnings = computed(() => {
    return this.myJobs()
      .filter(j =>
        j.status === 'Delivered' &&
        j.deliveredAt &&
        this.isToday(j.deliveredAt)
      )
      .reduce((s, j) => s + j.deliveryFee, 0);
  });

  todayDeliveries = computed(() =>
    this.myJobs().filter(j =>
      j.status === 'Delivered' &&
      j.deliveredAt &&
      this.isToday(j.deliveredAt)
    ).length
  );

  // Number of open jobs in the platform-wide pool this partner can claim.
  pendingJobs = computed(() => this.availableJobs().length);

  recentJobs = computed(() =>
    [...this.myJobs()]
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
      error: () => {
        this.loadingProfile.set(false);
        this.error.set('Could not load your profile. Please try again.');
      },
    });
  }

  private loadJobs(): void {
    let pending = 3;
    const done = () => { if (--pending === 0) this.loadingJobs.set(false); };
    const fail = (msg: string) => {
      this.error.set(msg);
      done();
    };

    this.deliverySvc.getMyJobHistory().subscribe({
      next: jobs => { this.myJobs.set(jobs); done(); },
      error: () => fail('Could not load your job history. Please try again.'),
    });

    this.deliverySvc.getActiveJobs().subscribe({
      next: jobs => { this.activeJobs.set(jobs); done(); },
      error: () => fail('Could not load your active jobs. Please try again.'),
    });

    this.deliverySvc.getJobs().subscribe({
      next: jobs => { this.availableJobs.set(jobs); done(); },
      error: () => fail('Could not load available jobs. Please try again.'),
    });
  }

  retry(): void {
    this.error.set(null);
    this.loadingProfile.set(true);
    this.loadingJobs.set(true);
    this.loadProfile();
    this.loadJobs();
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