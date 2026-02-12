import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportingService, SlaReport, VerificationBreakdown } from '../../core/services/reporting.service';

@Component({
  selector: 'app-sla-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styles: [],
  template: `
    <div>
      <!-- Header -->
      <div class="mb-8 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">SLA &amp; Compliance Reports</h1>
          <p class="mt-1 text-sm text-gray-500">Monitor service level agreements and compliance metrics</p>
        </div>
        <div class="flex flex-wrap gap-3">
          <select
            [(ngModel)]="selectedPeriod"
            (change)="loadReports()"
            class="rounded-lg border border-gray-300 px-3.5 py-2 text-sm text-gray-700 shadow-sm focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
            <option value="7">Last 7 Days</option>
            <option value="30">Last 30 Days</option>
            <option value="90">Last 90 Days</option>
            <option value="365">Last Year</option>
          </select>
          <button
            class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50"
            (click)="loadReports()">
            Refresh
          </button>
          <button
            class="rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-brand-600"
            (click)="exportReport()">
            Export CSV
          </button>
        </div>
      </div>

      @if (loading()) {
        <div class="py-16 text-center text-sm text-gray-400">Loading reports...</div>
      } @else {
        <!-- Overview grid -->
        <div class="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5">
          <!-- Gauge card -->
          <div class="flex items-center justify-center rounded-xl border border-gray-200 bg-white p-6">
            <div class="relative h-36 w-36">
              <svg viewBox="0 0 120 120" class="h-full w-full">
                <circle cx="60" cy="60" r="50" fill="none" stroke="#e5e7eb" stroke-width="10"/>
                <circle cx="60" cy="60" r="50" fill="none" stroke="#10b981" stroke-width="10"
                        [attr.stroke-dasharray]="getGaugeValues(sla()?.slaComplianceRate || 0)"
                        stroke-linecap="round"
                        transform="rotate(-90 60 60)"/>
              </svg>
              <div class="absolute inset-0 flex flex-col items-center justify-center">
                <span class="text-2xl font-bold text-gray-900">{{ sla()?.slaComplianceRate || 0 }}%</span>
                <span class="text-xs text-gray-500">SLA Compliance</span>
              </div>
            </div>
          </div>

          <div class="rounded-xl border border-gray-200 bg-white p-6 text-center">
            <p class="text-xs font-medium text-gray-500">Completed</p>
            <p class="mt-2 text-3xl font-bold text-success-700">{{ sla()?.completed || 0 }}</p>
            <p class="mt-1 text-xs text-gray-400">{{ sla()?.onTime || 0 }} on-time</p>
          </div>

          <div class="rounded-xl border border-gray-200 bg-white p-6 text-center">
            <p class="text-xs font-medium text-gray-500">Overdue</p>
            <p class="mt-2 text-3xl font-bold text-danger-700">{{ sla()?.overdue || 0 }}</p>
            <p class="mt-1 text-xs text-gray-400">{{ getOverduePercentage() }}% of completed</p>
          </div>

          <div class="rounded-xl border border-gray-200 bg-white p-6 text-center">
            <p class="text-xs font-medium text-gray-500">Avg. Completion</p>
            <p class="mt-2 text-3xl font-bold text-gray-900">{{ sla()?.averageDaysToComplete || 0 }}d</p>
            <p class="mt-1 text-xs text-gray-400">Target: {{ sla()?.targetCompletionDays || 0 }}d</p>
          </div>

          <div class="rounded-xl border border-gray-200 bg-white p-6 text-center">
            <p class="text-xs font-medium text-gray-500">Projected Rate</p>
            <p class="mt-2 text-3xl font-bold text-brand-600">{{ sla()?.projectedOnTimeRate || 0 }}%</p>
            <p class="mt-1 text-xs text-gray-400">Based on current pace</p>
          </div>
        </div>

        <!-- Breakdown section -->
        <div class="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <!-- Verification breakdown -->
          <div class="rounded-xl border border-gray-200 bg-white p-6">
            <h3 class="mb-5 text-base font-semibold text-gray-900">Verification Breakdown</h3>
            <div class="space-y-3">
              <div class="flex items-center gap-3">
                <span class="w-24 text-xs text-gray-500">Completed</span>
                <div class="h-2 flex-1 overflow-hidden rounded-full bg-gray-100">
                  <div class="h-full rounded-full bg-success-500 transition-all" [style.width.%]="getBreakdownPercentage('completed')"></div>
                </div>
                <span class="w-10 text-right text-xs font-medium">{{ verification()?.completed || 0 }}</span>
              </div>
              <div class="flex items-center gap-3">
                <span class="w-24 text-xs text-gray-500">In Progress</span>
                <div class="h-2 flex-1 overflow-hidden rounded-full bg-gray-100">
                  <div class="h-full rounded-full bg-brand-500 transition-all" [style.width.%]="getBreakdownPercentage('inProgress')"></div>
                </div>
                <span class="w-10 text-right text-xs font-medium">{{ verification()?.inProgress || 0 }}</span>
              </div>
              <div class="flex items-center gap-3">
                <span class="w-24 text-xs text-gray-500">Pending</span>
                <div class="h-2 flex-1 overflow-hidden rounded-full bg-gray-100">
                  <div class="h-full rounded-full bg-warning-500 transition-all" [style.width.%]="getBreakdownPercentage('pending')"></div>
                </div>
                <span class="w-10 text-right text-xs font-medium">{{ verification()?.pending || 0 }}</span>
              </div>
              <div class="flex items-center gap-3">
                <span class="w-24 text-xs text-gray-500">Failed</span>
                <div class="h-2 flex-1 overflow-hidden rounded-full bg-gray-100">
                  <div class="h-full rounded-full bg-danger-500 transition-all" [style.width.%]="getBreakdownPercentage('failed')"></div>
                </div>
                <span class="w-10 text-right text-xs font-medium">{{ verification()?.failed || 0 }}</span>
              </div>
            </div>
            <div class="mt-4 flex items-center justify-between border-t border-gray-100 pt-4 text-sm">
              <span class="text-gray-500">Completion Rate:</span>
              <span class="font-semibold text-success-700">{{ verification()?.completionRate || 0 }}%</span>
            </div>
          </div>

          <!-- By priority -->
          <div class="rounded-xl border border-gray-200 bg-white p-6">
            <h3 class="mb-5 text-base font-semibold text-gray-900">By Priority</h3>
            <table class="w-full">
              <thead>
                <tr class="border-b border-gray-200">
                  <th class="pb-2 text-left text-[11px] font-semibold uppercase tracking-wider text-gray-500">Priority</th>
                  <th class="pb-2 text-left text-[11px] font-semibold uppercase tracking-wider text-gray-500">Total</th>
                  <th class="pb-2 text-left text-[11px] font-semibold uppercase tracking-wider text-gray-500">Done</th>
                  <th class="pb-2 text-left text-[11px] font-semibold uppercase tracking-wider text-gray-500">Avg Days</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-gray-100">
                @for (item of sla()?.breakdownByPriority || []; track item.priority) {
                  <tr>
                    <td class="py-2.5">
                      <span class="inline-flex rounded-full px-2 py-0.5 text-[11px] font-medium"
                        [class]="getPriorityBadgeClasses(item.priority)">
                        {{ item.priorityName }}
                      </span>
                    </td>
                    <td class="py-2.5 text-xs text-gray-700">{{ item.total }}</td>
                    <td class="py-2.5 text-xs text-gray-700">{{ item.completed }}</td>
                    <td class="py-2.5 text-xs text-gray-700">{{ item.averageDays | number:'1.1-1' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <!-- By type -->
          <div class="rounded-xl border border-gray-200 bg-white p-6">
            <h3 class="mb-5 text-base font-semibold text-gray-900">By Verification Type</h3>
            <div class="space-y-4">
              @for (type of verification()?.byType || []; track type.typeValue) {
                <div class="border-b border-gray-50 pb-4 last:border-0 last:pb-0">
                  <div class="mb-2 flex items-center justify-between">
                    <span class="text-xs font-medium text-gray-700">{{ type.type }}</span>
                    <span class="text-xs text-gray-400">{{ type.completed }}/{{ type.total }}</span>
                  </div>
                  <div class="mb-2 h-1.5 overflow-hidden rounded-full bg-gray-100">
                    <div class="h-full rounded-full bg-indigo-500 transition-all" [style.width.%]="(type.completed / type.total) * 100"></div>
                  </div>
                  <div class="flex justify-between text-[11px] text-gray-400">
                    <span>Avg: {{ type.averageDurationHours | number:'1.1-1' }}h</span>
                    <span>{{ type.failed }} failed</span>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>
      }
    </div>
  `
})
export class SlaReportsComponent implements OnInit {
  private reportingService = inject(ReportingService);

  sla = signal<SlaReport | null>(null);
  verification = signal<VerificationBreakdown | null>(null);
  loading = signal(true);
  selectedPeriod = '30';

  ngOnInit() {
    this.loadReports();
  }

  loadReports() {
    this.loading.set(true);
    const days = parseInt(this.selectedPeriod);
    const fromDate = new Date();
    fromDate.setDate(fromDate.getDate() - days);
    const toDate = new Date();

    this.reportingService.getSlaReport(fromDate, toDate).subscribe({
      next: (report) => {
        this.sla.set(report);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    this.reportingService.getVerificationBreakdown().subscribe({
      next: (breakdown) => this.verification.set(breakdown)
    });
  }

  exportReport() {
    const days = parseInt(this.selectedPeriod);
    const fromDate = new Date();
    fromDate.setDate(fromDate.getDate() - days);
    const toDate = new Date();

    this.reportingService.exportSla(fromDate, toDate).subscribe({
      next: (blob) => {
        this.reportingService.downloadBlob(blob, `sla_report_${fromDate.toISOString().split('T')[0]}.csv`);
      }
    });
  }

  getGaugeValues(rate: number): string {
    const circumference = 2 * Math.PI * 50;
    const filled = (rate / 100) * circumference;
    return `${filled} ${circumference}`;
  }

  getOverduePercentage(): number {
    const total = (this.sla()?.completed || 0);
    const overdue = (this.sla()?.overdue || 0);
    return total > 0 ? Math.round((overdue / total) * 100) : 0;
  }

  getBreakdownPercentage(type: string): number {
    const total = this.verification()?.totalSteps || 1;
    const value = this.verification()?.[type as keyof VerificationBreakdown] as number || 0;
    return (value / total) * 100;
  }

  getPriorityBadgeClasses(priority: number): string {
    const map: Record<number, string> = {
      1: 'bg-green-50 text-green-700',
      2: 'bg-blue-50 text-blue-700',
      3: 'bg-red-50 text-red-700',
      4: 'bg-red-50 text-red-700'
    };
    return map[priority] || 'bg-blue-50 text-blue-700';
  }
}
