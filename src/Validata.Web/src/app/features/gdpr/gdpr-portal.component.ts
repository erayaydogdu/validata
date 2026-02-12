import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GDPRService, GDPRRequest, CreateGDPRRequest, Consent } from '../../core/services/gdpr.service';

@Component({
  selector: 'app-gdpr-portal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [],
  template: `
    <div>
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-gray-900">GDPR Portal</h1>
        <p class="mt-1 text-sm text-gray-500">Manage your data privacy requests</p>
      </div>

      <!-- Data Rights -->
      <div class="mb-6 rounded-xl border border-gray-200 bg-white p-6">
        <h2 class="mb-5 text-lg font-semibold text-gray-900">Your Data Rights</h2>
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <button
            class="rounded-lg border-2 border-gray-200 p-5 text-left transition hover:border-brand-500 hover:bg-brand-50/50"
            (click)="showRequestForm(1)">
            <span class="mb-3 block text-2xl">📥</span>
            <h3 class="mb-1 text-sm font-semibold text-gray-900">Data Export</h3>
            <p class="text-xs text-gray-500">Download a copy of all your personal data</p>
          </button>
          <button
            class="rounded-lg border-2 border-gray-200 p-5 text-left transition hover:border-brand-500 hover:bg-brand-50/50"
            (click)="showRequestForm(2)">
            <span class="mb-3 block text-2xl">🗑️</span>
            <h3 class="mb-1 text-sm font-semibold text-gray-900">Data Deletion</h3>
            <p class="text-xs text-gray-500">Request deletion of your personal data</p>
          </button>
          <button
            class="rounded-lg border-2 border-gray-200 p-5 text-left transition hover:border-brand-500 hover:bg-brand-50/50"
            (click)="showRequestForm(3)">
            <span class="mb-3 block text-2xl">✏️</span>
            <h3 class="mb-1 text-sm font-semibold text-gray-900">Data Rectification</h3>
            <p class="text-xs text-gray-500">Request correction of inaccurate data</p>
          </button>
          <button
            class="rounded-lg border-2 border-gray-200 p-5 text-left transition hover:border-brand-500 hover:bg-brand-50/50"
            (click)="showRequestForm(4)">
            <span class="mb-3 block text-2xl">⏸️</span>
            <h3 class="mb-1 text-sm font-semibold text-gray-900">Restrict Processing</h3>
            <p class="text-xs text-gray-500">Request restriction of data processing</p>
          </button>
        </div>
      </div>

      <!-- Request form -->
      @if (showForm()) {
        <div class="mb-6 rounded-xl border border-gray-200 bg-white p-6">
          <h2 class="mb-5 text-lg font-semibold text-gray-900">Submit Data Request</h2>
          <form (ngSubmit)="submitRequest()">
            <div class="mb-4">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Request Type</label>
              <input
                type="text"
                [value]="getRequestTypeName()"
                readonly
                class="block w-full rounded-lg border border-gray-300 bg-gray-50 px-3.5 py-2.5 text-sm text-gray-500">
            </div>
            <div class="mb-4">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Candidate ID</label>
              <input
                type="text"
                [(ngModel)]="candidateId"
                name="candidateId"
                required
                placeholder="Enter your candidate ID"
                class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
            </div>
            <div class="mb-5">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Reason (optional)</label>
              <textarea
                [(ngModel)]="reason"
                name="reason"
                rows="3"
                placeholder="Explain your request"
                class="block w-full resize-y rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"></textarea>
            </div>
            <div class="flex justify-end gap-3">
              <button
                type="button"
                class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50"
                (click)="cancelRequest()">
                Cancel
              </button>
              <button
                type="submit"
                class="rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white transition hover:bg-brand-600 disabled:opacity-50"
                [disabled]="isSubmitting()">
                {{ isSubmitting() ? 'Submitting...' : 'Submit Request' }}
              </button>
            </div>
          </form>
        </div>
      }

      <!-- Requests table -->
      <div class="mb-6 rounded-xl border border-gray-200 bg-white p-6">
        <h2 class="mb-5 text-lg font-semibold text-gray-900">My Requests</h2>
        @if (loading()) {
          <div class="py-10 text-center text-sm text-gray-400">Loading requests...</div>
        } @else if (requests().length === 0) {
          <div class="py-10 text-center text-sm text-gray-400">You haven't submitted any data requests yet.</div>
        } @else {
          <div class="overflow-x-auto">
            <table class="w-full">
              <thead>
                <tr class="border-b border-gray-200">
                  <th class="pb-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Request ID</th>
                  <th class="pb-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Type</th>
                  <th class="pb-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Status</th>
                  <th class="pb-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Requested</th>
                  <th class="pb-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Completed</th>
                  <th class="pb-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100">
                @for (req of requests(); track req.id) {
                  <tr>
                    <td class="py-3 font-mono text-xs text-gray-500">{{ req.id.substring(0, 8) }}...</td>
                    <td class="py-3 text-sm text-gray-900">{{ req.requestTypeName }}</td>
                    <td class="py-3">
                      <span class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium"
                        [class]="getStatusBadgeClasses(req.status)">
                        {{ req.statusName }}
                      </span>
                    </td>
                    <td class="py-3 text-sm text-gray-500">{{ req.requestedAt | date:'medium' }}</td>
                    <td class="py-3 text-sm text-gray-500">{{ req.completedAt ? (req.completedAt | date:'medium') : '-' }}</td>
                    <td class="py-3">
                      @if (req.requestType === 1 && req.status === 3) {
                        <button class="text-sm font-medium text-brand-600 hover:text-brand-500" (click)="downloadExport(req.id)">Download</button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>

      <!-- Consent management -->
      <div class="rounded-xl border border-gray-200 bg-white p-6">
        <h2 class="mb-5 text-lg font-semibold text-gray-900">Consent Management</h2>
        <div class="divide-y divide-gray-100">
          @for (consent of consents(); track consent.consentType) {
            <div class="flex items-center justify-between py-3">
              <div>
                <p class="text-sm font-medium text-gray-900">{{ consent.consentType }}</p>
                <p class="text-xs text-gray-500">Granted on {{ consent.grantedAt | date:'mediumDate' }}</p>
              </div>
              <span class="text-sm font-medium" [class.text-success-700]="consent.granted" [class.text-danger-700]="!consent.granted">
                {{ consent.granted ? 'Granted' : 'Denied' }}
              </span>
            </div>
          }
        </div>
      </div>
    </div>
  `
})
export class GDPRPortalComponent {
  private gdprService = inject(GDPRService);

  requests = signal<GDPRRequest[]>([]);
  consents = signal<Consent[]>([]);
  loading = signal(false);
  isSubmitting = signal(false);
  showForm = signal(false);
  requestType = signal(1);
  candidateId = '';
  reason = '';

  constructor() {
    this.loadRequests();
  }

  loadRequests() {
    this.loading.set(true);
    this.gdprService.getRequests().subscribe({
      next: (response) => {
        this.requests.set(response.data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  showRequestForm(type: number) {
    this.requestType.set(type);
    this.showForm.set(true);
  }

  getRequestTypeName(): string {
    const types: Record<number, string> = {
      1: 'Data Export',
      2: 'Data Deletion',
      3: 'Data Rectification',
      4: 'Restrict Processing'
    };
    return types[this.requestType()] || 'Unknown';
  }

  cancelRequest() {
    this.showForm.set(false);
    this.candidateId = '';
    this.reason = '';
  }

  submitRequest() {
    if (!this.candidateId) return;

    this.isSubmitting.set(true);
    const request: CreateGDPRRequest = {
      requestType: this.requestType(),
      candidateId: this.candidateId,
      reason: this.reason
    };

    this.gdprService.createRequest(request).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.cancelRequest();
        this.loadRequests();
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  downloadExport(requestId: string) {
    // Implementation for download
  }

  getStatusBadgeClasses(status: number): string {
    const map: Record<number, string> = {
      1: 'bg-amber-50 text-amber-700',
      2: 'bg-blue-50 text-blue-700',
      3: 'bg-green-50 text-green-700',
      4: 'bg-red-50 text-red-700'
    };
    return map[status] || 'bg-gray-50 text-gray-700';
  }
}
