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
  styles: [],
  template: `
    <div>
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-gray-900">Audit Log Viewer</h1>
        <p class="mt-1 text-sm text-gray-500">Track all system activities and changes</p>
      </div>

      <!-- Filters -->
      <div class="mb-6 flex flex-wrap gap-4 rounded-xl border border-gray-200 bg-white p-4">
        <div class="flex min-w-[150px] flex-col gap-1">
          <label class="text-xs font-medium text-gray-500">Search</label>
          <input
            type="text"
            [(ngModel)]="searchTerm"
            (input)="filterLogs()"
            placeholder="Search actions..."
            class="rounded-lg border border-gray-300 px-3 py-2 text-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
        </div>
        <div class="flex min-w-[150px] flex-col gap-1">
          <label class="text-xs font-medium text-gray-500">Action Type</label>
          <select
            [(ngModel)]="actionFilter"
            (change)="filterLogs()"
            class="rounded-lg border border-gray-300 px-3 py-2 text-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
            <option value="">All Actions</option>
            @for (action of uniqueActions(); track action) {
              <option [value]="action">{{ action }}</option>
            }
          </select>
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-xs font-medium text-gray-500">Date From</label>
          <input
            type="date"
            [(ngModel)]="dateFrom"
            (change)="filterLogs()"
            class="rounded-lg border border-gray-300 px-3 py-2 text-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-xs font-medium text-gray-500">Date To</label>
          <input
            type="date"
            [(ngModel)]="dateTo"
            (change)="filterLogs()"
            class="rounded-lg border border-gray-300 px-3 py-2 text-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
        </div>
      </div>

      <!-- Stats -->
      <div class="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div class="rounded-xl border border-gray-200 bg-white px-5 py-4">
          <p class="text-2xl font-bold text-gray-900">{{ filteredLogs().length }}</p>
          <p class="text-xs text-gray-500">Total Records</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white px-5 py-4">
          <p class="text-2xl font-bold text-gray-900">{{ uniqueActions().length }}</p>
          <p class="text-xs text-gray-500">Action Types</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white px-5 py-4">
          <p class="text-2xl font-bold text-gray-900">{{ getTodayCount() }}</p>
          <p class="text-xs text-gray-500">Today's Activity</p>
        </div>
      </div>

      @if (loading()) {
        <div class="py-16 text-center text-sm text-gray-400">Loading audit logs...</div>
      } @else if (filteredLogs().length === 0) {
        <div class="rounded-xl border border-gray-200 bg-white py-16 text-center text-sm text-gray-400">
          No audit logs found matching your criteria.
        </div>
      } @else {
        <!-- Table -->
        <div class="overflow-x-auto rounded-xl border border-gray-200 bg-white">
          <table class="w-full">
            <thead>
              <tr class="border-b border-gray-200 bg-gray-50">
                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Timestamp</th>
                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Action</th>
                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">User</th>
                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Entity</th>
                <th class="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-gray-500">Details</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100">
              @for (log of paginatedLogs(); track log.id) {
                <tr class="hover:bg-gray-50">
                  <td class="whitespace-nowrap px-4 py-3 text-xs text-gray-500">
                    {{ log.timestamp | date:'medium' }}
                  </td>
                  <td class="px-4 py-3">
                    <span class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium"
                      [class]="getActionBadgeClasses(log.action)">
                      {{ log.action }}
                    </span>
                  </td>
                  <td class="px-4 py-3 font-mono text-xs text-gray-500">{{ log.userId || 'System' }}</td>
                  <td class="px-4 py-3">
                    @if (log.entityType) {
                      <span class="text-sm font-medium text-gray-700">{{ log.entityType }}</span>
                      @if (log.entityId) {
                        <span class="ml-1 font-mono text-xs text-gray-400">{{ log.entityId.substring(0, 8) }}...</span>
                      }
                    } @else {
                      <span class="text-xs text-gray-300">-</span>
                    }
                  </td>
                  <td class="max-w-xs px-4 py-3">
                    @if (log.newValue) {
                      <span class="line-clamp-2 text-xs text-gray-600">{{ log.newValue }}</span>
                    } @else if (log.oldValue) {
                      <span class="line-clamp-2 text-xs text-gray-600">{{ log.oldValue }}</span>
                    } @else {
                      <span class="text-xs text-gray-300">-</span>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="mt-5 flex items-center justify-center gap-4">
          <button
            class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            (click)="prevPage()"
            [disabled]="currentPage() === 1">
            Previous
          </button>
          <span class="text-sm text-gray-500">Page {{ currentPage() }} of {{ totalPages() }}</span>
          <button
            class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
            (click)="nextPage()"
            [disabled]="currentPage() === totalPages()">
            Next
          </button>
        </div>
      }
    </div>
  `
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

  getActionBadgeClasses(action: string): string {
    if (action.includes('AUTH') || action.includes('LOGIN') || action.includes('LOGOUT')) return 'bg-blue-50 text-blue-700';
    if (action.includes('GDPR') || action.includes('DELETE') || action.includes('EXPORT')) return 'bg-amber-50 text-amber-700';
    if (action.includes('SCREENING') || action.includes('VERIFICATION')) return 'bg-green-50 text-green-700';
    if (action.includes('DOCUMENT') || action.includes('UPLOAD')) return 'bg-purple-50 text-purple-700';
    if (action.includes('CONSENT')) return 'bg-pink-50 text-pink-700';
    return 'bg-gray-50 text-gray-700';
  }
}
