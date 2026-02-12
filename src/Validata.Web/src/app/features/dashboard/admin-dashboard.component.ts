import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportingService, DashboardStats, KpiSummary, ScreeningTrends } from '../../core/services/reporting.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  styles: [],
  template: `
    <div>
      <!-- Header -->
      <div class="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900">Admin Dashboard</h1>
          <p class="mt-1 text-sm text-gray-500">Real-time overview of screening operations</p>
        </div>
        <div class="flex gap-3">
          <button
            class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50"
            (click)="refresh()">
            Refresh
          </button>
          <button
            class="rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-brand-600"
            (click)="exportReport()">
            Export Report
          </button>
        </div>
      </div>

      <!-- KPI Cards -->
      <div class="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div class="rounded-xl border border-gray-200 border-l-4 border-l-brand-500 bg-white p-5">
          <p class="text-2xl">📊</p>
          <p class="mt-2 text-2xl font-bold text-gray-900">{{ stats()?.activeScreenings || 0 }}</p>
          <p class="text-sm text-gray-500">Active Screenings</p>
          <p class="mt-1 text-xs text-success-700">↑ {{ stats()?.completedThisWeek || 0 }} this week</p>
        </div>
        <div class="rounded-xl border border-gray-200 border-l-4 border-l-green-500 bg-white p-5">
          <p class="text-2xl">✓</p>
          <p class="mt-2 text-2xl font-bold text-gray-900">{{ kpi()?.completionRate || 0 }}%</p>
          <p class="text-sm text-gray-500">Completion Rate</p>
          <p class="mt-1 text-xs" [class.text-success-700]="(kpi()?.weeklyChange || 0) > 0" [class.text-danger-700]="(kpi()?.weeklyChange || 0) < 0">
            {{ (kpi()?.weeklyChange || 0) > 0 ? '↑' : '↓' }} {{ kpi()?.weeklyChange || 0 }}% vs last week
          </p>
        </div>
        <div class="rounded-xl border border-gray-200 border-l-4 border-l-info-500 bg-white p-5">
          <p class="text-2xl">⏱️</p>
          <p class="mt-2 text-2xl font-bold text-gray-900">{{ stats()?.averageTurnaroundDays || 0 }}</p>
          <p class="text-sm text-gray-500">Avg Turnaround (days)</p>
          <p class="mt-1 text-xs text-gray-400">Target: 5 days</p>
        </div>
        <div class="rounded-xl border border-gray-200 border-l-4 border-l-warning-500 bg-white p-5">
          <p class="text-2xl">📋</p>
          <p class="mt-2 text-2xl font-bold text-gray-900">{{ stats()?.pendingActions || 0 }}</p>
          <p class="text-sm text-gray-500">Pending Actions</p>
          <p class="mt-1 text-xs text-gray-400">{{ stats()?.pendingVerification || 0 }} verifications</p>
        </div>
      </div>

      <!-- Secondary KPI row -->
      <div class="mb-8 grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div class="rounded-xl border border-gray-200 bg-white p-5 text-center">
          <p class="text-2xl">👥</p>
          <p class="mt-1 text-xl font-bold text-gray-900">{{ stats()?.totalCandidates || 0 }}</p>
          <p class="text-xs text-gray-500">Total Candidates</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white p-5 text-center">
          <p class="text-2xl">📄</p>
          <p class="mt-1 text-xl font-bold text-gray-900">{{ stats()?.uploadsToday || 0 }}</p>
          <p class="text-xs text-gray-500">Uploads Today</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white p-5 text-center">
          <p class="text-2xl">✅</p>
          <p class="mt-1 text-xl font-bold text-gray-900">{{ stats()?.completedToday || 0 }}</p>
          <p class="text-xs text-gray-500">Completed Today</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white p-5 text-center">
          <p class="text-2xl">📈</p>
          <p class="mt-1 text-xl font-bold text-gray-900">{{ kpi()?.averageSlaCompliance || 0 }}%</p>
          <p class="text-xs text-gray-500">SLA Compliance</p>
        </div>
      </div>

      <!-- Charts section -->
      <div class="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-3">
        <!-- Bar chart (2 cols) -->
        <div class="rounded-xl border border-gray-200 bg-white p-6 lg:col-span-2">
          <h3 class="mb-5 text-base font-semibold text-gray-900">Screening Trends (30 Days)</h3>
          @if (loading()) {
            <div class="flex h-52 items-center justify-center text-sm text-gray-400">Loading chart...</div>
          } @else {
            <div class="flex h-52 items-end gap-2 pb-8">
              @for (metric of chartData(); track $index) {
                <div class="flex flex-1 flex-col items-center">
                  <div class="flex items-end gap-1" style="height: 170px">
                    <div class="w-4 rounded-t bg-brand-500 transition-all" [style.height.%]="getBarHeight(metric.created)">
                      <span class="block text-center text-[10px] text-white">{{ metric.created }}</span>
                    </div>
                    <div class="w-4 rounded-t bg-success-500 transition-all" [style.height.%]="getBarHeight(metric.completed)">
                      <span class="block text-center text-[10px] text-white">{{ metric.completed }}</span>
                    </div>
                  </div>
                  <span class="mt-2 text-[10px] text-gray-400">{{ formatDate(metric.date) }}</span>
                </div>
              }
            </div>
            <div class="flex items-center justify-center gap-6">
              <span class="flex items-center gap-1.5 text-xs text-gray-500">
                <span class="inline-block h-3 w-3 rounded-sm bg-brand-500"></span> Created
              </span>
              <span class="flex items-center gap-1.5 text-xs text-gray-500">
                <span class="inline-block h-3 w-3 rounded-sm bg-success-500"></span> Completed
              </span>
            </div>
          }
        </div>

        <!-- Quick stats -->
        <div class="rounded-xl border border-gray-200 bg-white p-6">
          <h3 class="mb-5 text-base font-semibold text-gray-900">Quick Stats</h3>
          <div class="space-y-4">
            <div class="flex items-center justify-between border-b border-gray-100 pb-3">
              <span class="text-sm text-gray-500">Total Screenings</span>
              <span class="text-sm font-semibold text-gray-900">{{ kpi()?.totalScreenings || 0 }}</span>
            </div>
            <div class="flex items-center justify-between border-b border-gray-100 pb-3">
              <span class="text-sm text-gray-500">Pending</span>
              <span class="text-sm font-semibold text-gray-900">{{ kpi()?.pendingScreenings || 0 }}</span>
            </div>
            <div class="flex items-center justify-between border-b border-gray-100 pb-3">
              <span class="text-sm text-gray-500">Completed</span>
              <span class="text-sm font-semibold text-gray-900">{{ kpi()?.completedScreenings || 0 }}</span>
            </div>
            <div class="flex items-center justify-between border-b border-gray-100 pb-3">
              <span class="text-sm text-gray-500">Avg Processing Time</span>
              <span class="text-sm font-semibold text-gray-900">{{ kpi()?.averageCompletionTimeHours || 0 }}h</span>
            </div>
            <div class="flex items-center justify-between border-b border-gray-100 pb-3">
              <span class="text-sm text-gray-500">Documents Processed</span>
              <span class="text-sm font-semibold text-gray-900">{{ kpi()?.totalDocumentsProcessed || 0 }}</span>
            </div>
            <div class="flex items-center justify-between">
              <span class="text-sm text-gray-500">Growth Rate</span>
              <span class="text-sm font-semibold text-gray-900">{{ kpi()?.screeningGrowthRate || 0 }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Activity section -->
      <div class="rounded-xl border border-gray-200 bg-white p-6">
        <h3 class="mb-5 text-base font-semibold text-gray-900">Recent Activity</h3>
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div class="flex gap-3 rounded-lg bg-gray-50 p-4">
            <span class="text-2xl">📋</span>
            <div>
              <p class="text-sm font-medium text-gray-900">{{ stats()?.activeScreenings || 0 }} Active Screenings</p>
              <p class="text-xs text-gray-500">Currently in progress</p>
            </div>
          </div>
          <div class="flex gap-3 rounded-lg bg-gray-50 p-4">
            <span class="text-2xl">⏳</span>
            <div>
              <p class="text-sm font-medium text-gray-900">{{ stats()?.pendingVerification || 0 }} Pending Verifications</p>
              <p class="text-xs text-gray-500">Awaiting action</p>
            </div>
          </div>
          <div class="flex gap-3 rounded-lg bg-gray-50 p-4">
            <span class="text-2xl">📤</span>
            <div>
              <p class="text-sm font-medium text-gray-900">{{ stats()?.uploadsToday || 0 }} Documents Uploaded</p>
              <p class="text-xs text-gray-500">Today</p>
            </div>
          </div>
          <div class="flex gap-3 rounded-lg bg-gray-50 p-4">
            <span class="text-2xl">🎯</span>
            <div>
              <p class="text-sm font-medium text-gray-900">{{ kpi()?.completionRate || 0 }}% Success Rate</p>
              <p class="text-xs text-gray-500">Overall completion</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdminDashboardComponent implements OnInit {
  private reportingService = inject(ReportingService);

  stats = signal<DashboardStats | null>(null);
  kpi = signal<KpiSummary | null>(null);
  trends = signal<ScreeningTrends | null>(null);
  chartData = signal<any[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.loading.set(true);

    this.reportingService.getDashboardStats().subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => this.loading.set(false)
    });

    this.reportingService.getKpiSummary().subscribe({
      next: (kpi) => this.kpi.set(kpi),
      error: () => this.loading.set(false)
    });

    this.reportingService.getScreeningTrends(14).subscribe({
      next: (trends) => {
        this.trends.set(trends);
        this.chartData.set(trends.dailyMetrics.slice(-14));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  refresh() {
    this.loadData();
  }

  exportReport() {
    this.reportingService.exportKpi().subscribe({
      next: (blob) => {
        this.reportingService.downloadBlob(blob, `kpi_report_${new Date().toISOString().split('T')[0]}.csv`);
      }
    });
  }

  getBarHeight(value: number): number {
    const max = Math.max(...this.chartData().map(d => Math.max(d.created, d.completed)), 1);
    return Math.max((value / max) * 100, 5);
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
