import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryService, DeliveryJob } from '../../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-jobs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delivery-jobs.component.html',
  styleUrls: ['./delivery-jobs.component.scss'],
})
export class DeliveryJobsComponent implements OnInit {

  private svc = inject(DeliveryService);

  loading    = signal(true);
  actionId   = signal<string | null>(null);
  errorMsg   = signal<string | null>(null);
  successMsg = signal<string | null>(null);

  assignedJobs  = signal<DeliveryJob[]>([]);
  completedJobs = signal<DeliveryJob[]>([]);
  activeTab     = signal<'assigned' | 'completed'>('assigned');

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
        this.toast('success', 'Job accepted! Head to the restaurant.');
        this.loadJobs();
      },
      error: () => { this.actionId.set(null); this.toast('error', 'Failed to accept job.'); },
    });
  }

  reject(job: DeliveryJob): void {
    this.actionId.set(job.id);
    // JobStatus.Rejected = 5
    this.svc.updateJobStatus(job.id, 5).subscribe({
      next: () => { this.actionId.set(null); this.loadJobs(); },
      error: () => { this.actionId.set(null); this.toast('error', 'Failed to reject job.'); },
    });
  }

  timeAgo(dateStr: string): string {
    const mins = Math.floor((Date.now() - new Date(dateStr).getTime()) / 60000);
    if (mins < 1)  return 'Just now';
    if (mins < 60) return `${mins}m ago`;
    return `${Math.floor(mins / 60)}h ago`;
  }

  private toast(type: 'success' | 'error', msg: string): void {
    if (type === 'success') { this.successMsg.set(msg); setTimeout(() => this.successMsg.set(null), 3000); }
    else                    { this.errorMsg.set(msg);   setTimeout(() => this.errorMsg.set(null),   4000); }
  }
}