import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GDPRService, GDPRRequest, CreateGDPRRequest } from '../../core/services/gdpr.service';

@Component({
  selector: 'app-gdpr-portal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="gdpr-portal">
      <header class="portal-header">
        <h1>GDPR Portal</h1>
        <p class="subtitle">Manage your data privacy requests</p>
      </header>

      <div class="portal-content">
        <section class="data-rights">
          <h2>Your Data Rights</h2>
          <div class="rights-grid">
            <div class="right-card" (click)="showRequestForm(1)">
              <div class="right-icon">📥</div>
              <h3>Data Export</h3>
              <p>Download a copy of all your personal data</p>
            </div>
            <div class="right-card" (click)="showRequestForm(2)">
              <div class="right-icon">🗑️</div>
              <h3>Data Deletion</h3>
              <p>Request deletion of your personal data</p>
            </div>
            <div class="right-card" (click)="showRequestForm(3)">
              <div class="right-icon">✏️</div>
              <h3>Data Rectification</h3>
              <p>Request correction of inaccurate data</p>
            </div>
            <div class="right-card" (click)="showRequestForm(4)">
              <div class="right-icon">⏸️</div>
              <h3>Restrict Processing</h3>
              <p>Request restriction of data processing</p>
            </div>
          </div>
        </section>

        @if (showForm()) {
          <section class="request-form">
            <h2>Submit Data Request</h2>
            <form (ngSubmit)="submitRequest()">
              <div class="form-group">
                <label>Request Type</label>
                <input type="text" [value]="getRequestTypeName()" readonly class="readonly-input">
              </div>
              <div class="form-group">
                <label>Candidate ID</label>
                <input type="text" [(ngModel)]="candidateId" name="candidateId" required placeholder="Enter your candidate ID">
              </div>
              <div class="form-group">
                <label>Reason (optional)</label>
                <textarea [(ngModel)]="reason" name="reason" rows="3" placeholder="Explain your request"></textarea>
              </div>
              <div class="form-actions">
                <button type="button" class="btn-secondary" (click)="cancelRequest()">Cancel</button>
                <button type="submit" class="btn-primary" [disabled]="isSubmitting()">
                  {{ isSubmitting() ? 'Submitting...' : 'Submit Request' }}
                </button>
              </div>
            </form>
          </section>
        }

        <section class="my-requests">
          <h2>My Requests</h2>
          @if (loading()) {
            <div class="loading">Loading requests...</div>
          } @else if (requests().length === 0) {
            <div class="empty-state">
              <p>You haven't submitted any data requests yet.</p>
            </div>
          } @else {
            <table class="requests-table">
              <thead>
                <tr>
                  <th>Request ID</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Requested</th>
                  <th>Completed</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (req of requests(); track req.id) {
                  <tr>
                    <td>{{ req.id.substring(0, 8) }}...</td>
                    <td>{{ req.requestTypeName }}</td>
                    <td>
                      <span class="status-badge" [class]="getStatusClass(req.status)">
                        {{ req.statusName }}
                      </span>
                    </td>
                    <td>{{ req.requestedAt | date:'medium' }}</td>
                    <td>{{ req.completedAt ? (req.completedAt | date:'medium') : '-' }}</td>
                    <td>
                      @if (req.requestType === 1 && req.status === 3) {
                        <button class="btn-link" (click)="downloadExport(req.id)">Download</button>
                      }
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </section>

        <section class="consent-section">
          <h2>Consent Management</h2>
          <div class="consent-list">
            @for (consent of consents(); track consent.consentType) {
              <div class="consent-item">
                <div class="consent-info">
                  <strong>{{ consent.consentType }}</strong>
                  <span class="consent-date">Granted on {{ consent.grantedAt | date:'mediumDate' }}</span>
                </div>
                <span class="consent-status" [class.granted]="consent.granted">
                  {{ consent.granted ? 'Granted' : 'Denied' }}
                </span>
              </div>
            }
          </div>
        </section>
      </div>
    </div>
  `,
  styles: [`
    .gdpr-portal {
      max-width: 1200px;
      margin: 0 auto;
      padding: 24px;
    }
    .portal-header {
      margin-bottom: 32px;
    }
    .portal-header h1 {
      font-size: 28px;
      font-weight: 600;
      color: #1a1a1a;
      margin: 0 0 8px 0;
    }
    .subtitle {
      color: #666;
      margin: 0;
    }
    section {
      background: #fff;
      border-radius: 8px;
      padding: 24px;
      margin-bottom: 24px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    section h2 {
      font-size: 18px;
      font-weight: 600;
      margin: 0 0 20px 0;
      color: #1a1a1a;
    }
    .rights-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
    }
    .right-card {
      border: 2px solid #e5e7eb;
      border-radius: 8px;
      padding: 20px;
      cursor: pointer;
      transition: all 0.2s;
    }
    .right-card:hover {
      border-color: #3b82f6;
      background: #f8fafc;
    }
    .right-icon {
      font-size: 32px;
      margin-bottom: 12px;
    }
    .right-card h3 {
      font-size: 16px;
      font-weight: 600;
      margin: 0 0 8px 0;
    }
    .right-card p {
      font-size: 14px;
      color: #666;
      margin: 0;
    }
    .form-group {
      margin-bottom: 16px;
    }
    .form-group label {
      display: block;
      font-size: 14px;
      font-weight: 500;
      margin-bottom: 6px;
      color: #374151;
    }
    .form-group input,
    .form-group textarea,
    .readonly-input {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 14px;
      box-sizing: border-box;
    }
    .readonly-input {
      background: #f3f4f6;
      color: #6b7280;
    }
    .form-group textarea {
      resize: vertical;
    }
    .form-actions {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
    }
    .btn-primary,
    .btn-secondary {
      padding: 10px 20px;
      border-radius: 6px;
      font-size: 14px;
      font-weight: 500;
      cursor: pointer;
    }
    .btn-primary {
      background: #3b82f6;
      color: white;
      border: none;
    }
    .btn-primary:disabled {
      background: #93c5fd;
    }
    .btn-secondary {
      background: white;
      color: #374151;
      border: 1px solid #d1d5db;
    }
    .btn-link {
      background: none;
      border: none;
      color: #3b82f6;
      cursor: pointer;
      font-size: 14px;
    }
    .requests-table {
      width: 100%;
      border-collapse: collapse;
    }
    .requests-table th,
    .requests-table td {
      padding: 12px;
      text-align: left;
      border-bottom: 1px solid #e5e7eb;
    }
    .requests-table th {
      font-weight: 600;
      font-size: 13px;
      color: #6b7280;
      text-transform: uppercase;
    }
    .status-badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 9999px;
      font-size: 12px;
      font-weight: 500;
    }
    .status-badge.pending { background: #fef3c7; color: #92400e; }
    .status-badge.processing { background: #dbeafe; color: #1e40af; }
    .status-badge.completed { background: #d1fae5; color: #065f46; }
    .status-badge.rejected { background: #fee2e2; color: #991b1b; }
    .consent-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid #e5e7eb;
    }
    .consent-info strong {
      display: block;
      font-weight: 500;
    }
    .consent-date {
      font-size: 13px;
      color: #6b7280;
    }
    .consent-status {
      font-size: 14px;
      color: #dc2626;
    }
    .consent-status.granted {
      color: #059669;
    }
    .loading {
      text-align: center;
      padding: 40px;
      color: #6b7280;
    }
    .empty-state {
      text-align: center;
      padding: 40px;
      color: #6b7280;
    }
  `]
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

  getStatusClass(status: number): string {
    const classes: Record<number, string> = {
      1: 'pending',
      2: 'processing',
      3: 'completed',
      4: 'rejected'
    };
    return classes[status] || '';
  }
}
