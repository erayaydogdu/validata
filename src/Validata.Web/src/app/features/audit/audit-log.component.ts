import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface AuditLog {
  id: string;
  userId?: string;
  action: string;
  entityType?: string;
  entityId?: string;
  oldValue?: string;
  newValue?: string;
  timestamp: Date;
}

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="audit-viewer">
      <header class="page-header">
        <h1>Audit Log Viewer</h1>
        <p class="subtitle">Track all system activities and changes</p>
      </header>

      <div class="filters">
        <div class="filter-group">
          <label>Search</label>
          <input type="text" [(ngModel)]="searchTerm" (input)="filterLogs()" placeholder="Search actions...">
        </div>
        <div class="filter-group">
          <label>Action Type</label>
          <select [(ngModel)]="actionFilter" (change)="filterLogs()">
            <option value="">All Actions</option>
            @for (action of uniqueActions(); track action) {
              <option [value]="action">{{ action }}</option>
            }
          </select>
        </div>
        <div class="filter-group">
          <label>Date From</label>
          <input type="date" [(ngModel)]="dateFrom" (change)="filterLogs()">
        </div>
        <div class="filter-group">
          <label>Date To</label>
          <input type="date" [(ngModel)]="dateTo" (change)="filterLogs()">
        </div>
      </div>

      <div class="log-stats">
        <div class="stat-card">
          <span class="stat-value">{{ filteredLogs().length }}</span>
          <span class="stat-label">Total Records</span>
        </div>
        <div class="stat-card">
          <span class="stat-value">{{ uniqueActions().length }}</span>
          <span class="stat-label">Action Types</span>
        </div>
        <div class="stat-card">
          <span class="stat-value">{{ getTodayCount() }}</span>
          <span class="stat-label">Today's Activity</span>
        </div>
      </div>

      @if (loading()) {
        <div class="loading">Loading audit logs...</div>
      } @else if (filteredLogs().length === 0) {
        <div class="empty-state">
          <p>No audit logs found matching your criteria.</p>
        </div>
      } @else {
        <div class="logs-table-container">
          <table class="logs-table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Action</th>
                <th>User</th>
                <th>Entity</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              @for (log of paginatedLogs(); track log.id) {
                <tr>
                  <td class="timestamp">
                    {{ log.timestamp | date:'medium' }}
                  </td>
                  <td>
                    <span class="action-badge" [class]="getActionClass(log.action)">
                      {{ log.action }}
                    </span>
                  </td>
                  <td class="user-id">{{ log.userId || 'System' }}</td>
                  <td>
                    @if (log.entityType) {
                      <span class="entity">{{ log.entityType }}</span>
                      @if (log.entityId) {
                        <span class="entity-id">{{ log.entityId.substring(0, 8) }}...</span>
                      }
                    } @else {
                      -
                    }
                  </td>
                  <td class="details">
                    @if (log.newValue) {
                      <span class="detail-value">{{ log.newValue }}</span>
                    } @else if (log.oldValue) {
                      <span class="detail-value">{{ log.oldValue }}</span>
                    } @else {
                      -
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="pagination">
          <button class="btn-page" (click)="prevPage()" [disabled]="currentPage() === 1">
            Previous
          </button>
          <span class="page-info">Page {{ currentPage() }} of {{ totalPages() }}</span>
          <button class="btn-page" (click)="nextPage()" [disabled]="currentPage() === totalPages()">
            Next
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .audit-viewer {
      max-width: 1400px;
      margin: 0 auto;
      padding: 24px;
    }
    .page-header {
      margin-bottom: 24px;
    }
    .page-header h1 {
      font-size: 28px;
      font-weight: 600;
      margin: 0 0 8px 0;
    }
    .subtitle {
      color: #666;
      margin: 0;
    }
    .filters {
      display: flex;
      gap: 16px;
      flex-wrap: wrap;
      margin-bottom: 24px;
      padding: 16px;
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    .filter-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .filter-group label {
      font-size: 12px;
      font-weight: 500;
      color: #6b7280;
    }
    .filter-group input,
    .filter-group select {
      padding: 8px 12px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 14px;
      min-width: 150px;
    }
    .log-stats {
      display: flex;
      gap: 16px;
      margin-bottom: 24px;
    }
    .stat-card {
      background: #fff;
      padding: 16px 24px;
      border-radius: 8px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .stat-value {
      font-size: 24px;
      font-weight: 600;
      color: #1a1a1a;
    }
    .stat-label {
      font-size: 13px;
      color: #6b7280;
    }
    .logs-table-container {
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
      overflow-x: auto;
    }
    .logs-table {
      width: 100%;
      border-collapse: collapse;
    }
    .logs-table th,
    .logs-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid #e5e7eb;
    }
    .logs-table th {
      font-weight: 600;
      font-size: 12px;
      text-transform: uppercase;
      color: #6b7280;
      background: #f9fafb;
    }
    .timestamp {
      white-space: nowrap;
      font-size: 13px;
      color: #666;
    }
    .action-badge {
      display: inline-block;
      padding: 4px 10px;
      border-radius: 9999px;
      font-size: 12px;
      font-weight: 500;
    }
    .action-badge.auth { background: #dbeafe; color: #1e40af; }
    .action-badge.gdpr { background: #fef3c7; color: #92400e; }
    .action-badge.screening { background: #d1fae5; color: #065f46; }
    .action-badge.document { background: #ede9fe; color: #5b21b6; }
    .action-badge.consent { background: #fce7f3; color: #9d174d; }
    .user-id, .entity-id {
      font-family: monospace;
      font-size: 12px;
      color: #6b7280;
    }
    .entity {
      font-weight: 500;
      margin-right: 4px;
    }
    .details {
      max-width: 300px;
    }
    .detail-value {
      font-size: 13px;
      color: #374151;
      overflow: hidden;
      text-overflow: ellipsis;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
    }
    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 16px;
      margin-top: 20px;
    }
    .btn-page {
      padding: 8px 16px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      background: #fff;
      cursor: pointer;
      font-size: 14px;
    }
    .btn-page:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .btn-page:hover:not(:disabled) {
      background: #f3f4f6;
    }
    .page-info {
      font-size: 14px;
      color: #6b7280;
    }
    .loading {
      text-align: center;
      padding: 60px;
      color: #6b7280;
    }
    .empty-state {
      text-align: center;
      padding: 60px;
      color: #6b7280;
      background: #fff;
      border-radius: 8px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
  `]
})
export class AuditLogComponent {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/audit`;

  logs = signal<AuditLog[]>([]);
  filteredLogs = signal<AuditLog[]>([]);
  loading = signal(false);
  currentPage = signal(1);
  pageSize = 20;

  searchTerm = '';
  actionFilter = '';
  dateFrom = '';
  dateTo = '';

  uniqueActions(): string[] {
    const actions = new Set(this.logs().map(l => l.action));
    return Array.from(actions).sort();
  }

  constructor() {
    this.loadLogs();
  }

  loadLogs() {
    this.loading.set(true);
    this.http.get<AuditLog[]>(this.baseUrl).subscribe({
      next: (logs) => {
        this.logs.set(logs.sort((a, b) => 
          new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
        ));
        this.filterLogs();
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  filterLogs() {
    let filtered = this.logs();

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(l => 
        l.action.toLowerCase().includes(term) ||
        l.newValue?.toLowerCase().includes(term) ||
        l.oldValue?.toLowerCase().includes(term)
      );
    }

    if (this.actionFilter) {
      filtered = filtered.filter(l => l.action === this.actionFilter);
    }

    if (this.dateFrom) {
      const from = new Date(this.dateFrom);
      filtered = filtered.filter(l => new Date(l.timestamp) >= from);
    }

    if (this.dateTo) {
      const to = new Date(this.dateTo);
      to.setHours(23, 59, 59, 999);
      filtered = filtered.filter(l => new Date(l.timestamp) <= to);
    }

    this.filteredLogs.set(filtered);
    this.currentPage.set(1);
  }

  paginatedLogs(): AuditLog[] {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.filteredLogs().slice(start, start + this.pageSize);
  }

  totalPages(): number {
    return Math.ceil(this.filteredLogs().length / this.pageSize);
  }

  getTodayCount(): number {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return this.logs().filter(l => new Date(l.timestamp) >= today).length;
  }

  prevPage() {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
    }
  }

  nextPage() {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(p => p + 1);
    }
  }

  getActionClass(action: string): string {
    if (action.includes('AUTH') || action.includes('LOGIN') || action.includes('LOGOUT')) return 'auth';
    if (action.includes('GDPR') || action.includes('DELETE') || action.includes('EXPORT')) return 'gdpr';
    if (action.includes('SCREENING') || action.includes('VERIFICATION')) return 'screening';
    if (action.includes('DOCUMENT') || action.includes('UPLOAD')) return 'document';
    if (action.includes('CONSENT')) return 'consent';
    return '';
  }
}
