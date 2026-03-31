import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryJob, DeliveryService } from '../../../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-active',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delivery-active.component.html',
  styleUrls: ['./delivery-active.component.scss'],
})
export class DeliveryActiveComponent implements OnInit {

  private svc = inject(DeliveryService);

  loading    = signal(true);
  actionId   = signal<string | null>(null);
  successMsg = signal<string | null>(null);
  errorMsg   = signal<string | null>(null);
  activeJobs = signal<DeliveryJob[]>([]);

  ngOnInit(): void { this.loadActive(); }

  loadActive(): void {
    this.loading.set(true);
    this.svc.getActiveJobs().subscribe({
      next:  jobs => { this.activeJobs.set(jobs); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }

  markPickedUp(job: DeliveryJob): void {
    // JobStatus.PickedUp = 3
    this.updateStatus(job.id, 3, 'Marked as picked up!');
  }

  markDelivered(job: DeliveryJob): void {
    // JobStatus.Delivered = 4
    this.updateStatus(job.id, 4, 'Delivery completed! Great work 🎉');
  }

  private updateStatus(jobId: string, status: number, successText: string): void {
    this.actionId.set(jobId);
    this.svc.updateJobStatus(jobId, status).subscribe({
      next: () => {
        this.actionId.set(null);
        this.toast('success', successText);
        this.loadActive();
      },
      error: () => { this.actionId.set(null); this.toast('error', 'Failed to update status.'); },
    });
  }

  nextAction(job: DeliveryJob): { label: string; fn: () => void } | null {
    if (job.status === 'Accepted')  return { label: 'Mark Picked Up',  fn: () => this.markPickedUp(job)  };
    if (job.status === 'PickedUp')  return { label: 'Mark Delivered',  fn: () => this.markDelivered(job) };
    return null;
  }

  statusStep(job: DeliveryJob): number {
    const steps: Record<string, number> = {
      Assigned: 0, Accepted: 1, PickedUp: 2, Delivered: 3
    };
    return steps[job.status] ?? 0;
  }

  private toast(type: 'success' | 'error', msg: string): void {
    if (type === 'success') { this.successMsg.set(msg); setTimeout(() => this.successMsg.set(null), 3000); }
    else                    { this.errorMsg.set(msg);   setTimeout(() => this.errorMsg.set(null),   4000); }
  }
}