import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeliveryJob, DeliveryService } from '../../../../core/services/delivery.service';

@Component({
  selector: 'app-delivery-active',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './delivery-active.component.html',
  styleUrls: ['./delivery-active.component.scss'],
})
export class DeliveryActiveComponent implements OnInit, OnDestroy {

  private svc = inject(DeliveryService);

  loading    = signal(true);
  actionId   = signal<string | null>(null);
  successMsg = signal<string | null>(null);
  errorMsg   = signal<string | null>(null);
  activeJobs = signal<DeliveryJob[]>([]);

  // ✅ Location tracking
  private locationInterval: any;
  isTracking = signal(false);

  ngOnInit(): void { this.loadActive(); }

  ngOnDestroy(): void {
    this.stopSendingLocation(); // ✅ cleanup on destroy
  }

  loadActive(): void {
    this.loading.set(true);
    this.svc.getActiveJobs().subscribe({
      next: jobs => {
        this.activeJobs.set(jobs);
        this.loading.set(false);

        // ✅ Auto-start location if active job exists
        const hasActiveJob = jobs.some(
          j => j.status === 'Accepted' || j.status === 'PickedUp'
        );
        if (hasActiveJob && !this.isTracking()) {
          this.startSendingLocation();
        }
        if (!hasActiveJob) {
          this.stopSendingLocation();
        }
      },
      error: () => this.loading.set(false),
    });
  }

  // ✅ Send GPS location every 5 seconds — like Swiggy
  startSendingLocation(): void {
    if (this.isTracking()) return;

    if (!navigator.geolocation) {
      console.warn('Geolocation not supported');
      return;
    }

    this.isTracking.set(true);
    console.log('🛵 Location tracking started');

    this.locationInterval = setInterval(() => {
      navigator.geolocation.getCurrentPosition(
        pos => {
          this.svc.updateLocation(
            pos.coords.latitude,
            pos.coords.longitude
          ).subscribe({
            next: () => console.log(
              '📍 Location sent:',
              pos.coords.latitude,
              pos.coords.longitude
            ),
            error: err => console.error('Location update failed:', err)
          });
        },
        err => console.warn('Geolocation error:', err),
        { enableHighAccuracy: true, timeout: 5000 }
      );
    }, 5000);
  }

  stopSendingLocation(): void {
    if (this.locationInterval) {
      clearInterval(this.locationInterval);
      this.locationInterval = null;
      this.isTracking.set(false);
      console.log('🛵 Location tracking stopped');
    }
  }

  markPickedUp(job: DeliveryJob): void {
    this.updateStatus(job.id, 3, 'Marked as picked up!');
  }

  markDelivered(job: DeliveryJob): void {
    this.updateStatus(job.id, 4, 'Delivery completed! Great work 🎉');
  }

  private updateStatus(
    jobId: string, status: number, successText: string): void {
    this.actionId.set(jobId);
    this.svc.updateJobStatus(jobId, status).subscribe({
      next: () => {
        this.actionId.set(null);
        this.toast('success', successText);
        this.loadActive(); // ← reloads and re-checks if tracking needed
      },
      error: () => {
        this.actionId.set(null);
        this.toast('error', 'Failed to update status.');
      },
    });
  }

  nextAction(job: DeliveryJob): { label: string; fn: () => void } | null {
    if (job.status === 'Accepted') return { label: 'Mark Picked Up',  fn: () => this.markPickedUp(job) };
    if (job.status === 'PickedUp') return { label: 'Mark Delivered',  fn: () => this.markDelivered(job) };
    return null;
  }

  statusStep(job: DeliveryJob): number {
    const steps: Record<string, number> = {
      Assigned: 0, Accepted: 1, PickedUp: 2, Delivered: 3
    };
    return steps[job.status] ?? 0;
  }

  private toast(type: 'success' | 'error', msg: string): void {
    if (type === 'success') {
      this.successMsg.set(msg);
      setTimeout(() => this.successMsg.set(null), 3000);
    } else {
      this.errorMsg.set(msg);
      setTimeout(() => this.errorMsg.set(null), 4000);
    }
  }
}