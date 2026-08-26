import { Component, OnInit, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryService, DeliveryJob } from '../../../core/services/delivery.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-delivery-jobs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delivery-jobs.component.html',
  styleUrls: ['./delivery-jobs.component.scss'],
})
export class DeliveryJobsComponent implements OnInit {

  private svc        = inject(DeliveryService);
  private toastSvc   = inject(ToastService);
  private confirmSvc = inject(ConfirmService);
  private notifSvc   = inject(NotificationService);

  loading    = signal(true);
  actionId   = signal<string | null>(null);

  assignedJobs  = signal<DeliveryJob[]>([]);
  completedJobs = signal<DeliveryJob[]>([]);
  activeTab     = signal<'assigned' | 'completed'>('assigned');

  constructor() {
    // 📦 New job pushed via SignalR — refresh the list instantly instead of
    // waiting for the user to navigate away/back or refresh the page.
    effect(() => {
      const job = this.notifSvc.newJobAvailable();
      if (!job) return;
      this.toastSvc.success('New job available!');
      this.loadJobs();
    });
  }

  ngOnInit(): void { this.loadJobs(); }

  loadJobs(): void {
    this.loading.set(true);
    this.svc.getJobs().subscribe({
      next: jobs => {
        this.assignedJobs.set(jobs.filter(j => j.status === 'Assigned'));
        this.completedJobs.set(jobs.filter(j => j.status === 'Delivered' || j.status === 'Rejected'));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  accept(job: DeliveryJob): void {
    this.actionId.set(job.id);
    this.svc.acceptJob(job.id).subscribe({
      next: updated => {
        this.assignedJobs.update(list =>
          list.map(j => j.id === job.id ? updated : j)
        );
        this.actionId.set(null);
        this.toastSvc.success('Job accepted! Head to the restaurant.');
        this.loadJobs();
      },
      error: () => { this.actionId.set(null); this.toastSvc.error('Failed to accept job.'); },
    });
  }

  async reject(job: DeliveryJob): Promise<void> {
    const ok = await this.confirmSvc.confirm({
      title: 'Reject this job?',
      message: 'It\'ll go back into the queue for another partner to pick up.',
      confirmLabel: 'Reject job',
      danger: true,
    });
    if (!ok) return;
    this.actionId.set(job.id);
    // JobStatus.Rejected = 5
    this.svc.updateJobStatus(job.id, 5).subscribe({
      next: () => { this.actionId.set(null); this.toastSvc.success('Job rejected.'); this.loadJobs(); },
      error: () => { this.actionId.set(null); this.toastSvc.error('Failed to reject job.'); },
    });
  }

  timeAgo(dateStr: string): string {
    const mins = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    return `${Math.floor(mins / 60)}h ago`;
  }
}