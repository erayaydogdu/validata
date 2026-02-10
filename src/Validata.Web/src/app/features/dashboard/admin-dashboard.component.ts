import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportingService, DashboardStats, KpiSummary, ScreeningTrends } from '../../core/services/reporting.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard">
      <header class="dashboard-header">
        <div class="header-content">
          <h1>Admin Dashboard</h1>
          <p class="subtitle">Real-time overview of screening operations</p>
        </div>
        <div class="header-actions">
          <button class="btn-secondary" (click)="refresh()">Refresh</button>
          <button class="btn-primary" (click)="exportReport()">Export Report</button>
        </div>
      </header>

      <section class="kpi-grid">
        <div class="kpi-card primary">
          <div class="kpi-icon">📊</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ stats()?.activeScreenings || 0 }}</span>
            <span class="kpi-label">Active Screenings</span>
          </div>
          <div class="kpi-trend up">
            <span>↑ {{ stats()?.completedThisWeek || 0 }} this week</span>
          </div>
        </div>

        <div class="kpi-card success">
          <div class="kpi-icon">✓</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ kpi()?.completionRate || 0 }}%</span>
            <span class="kpi-label">Completion Rate</span>
          </div>
          <div class="kpi-trend" [class.up]="(kpi()?.weeklyChange || 0) > 0" [class.down]="(kpi()?.weeklyChange || 0) < 0">
            <span>{{ (kpi()?.weeklyChange || 0) > 0 ? '↑' : '↓' }} {{ kpi()?.weeklyChange || 0 }}% vs last week</span>
          </div>
        </div>

        <div class="kpi-card info">
          <div class="kpi-icon">⏱️</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ stats()?.averageTurnaroundDays || 0 }}</span>
            <span class="kpi-label">Avg Turnaround (days)</span>
          </div>
          <div class="kpi-meta">Target: 5 days</div>
        </div>

        <div class="kpi-card warning">
          <div class="kpi-icon">📋</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ stats()?.pendingActions || 0 }}</span>
            <span class="kpi-label">Pending Actions</span>
          </div>
          <div class="kpi-meta">{{ stats()?.pendingVerification || 0 }} verifications</div>
        </div>

        <div class="kpi-card">
          <div class="kpi-icon">👥</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ stats()?.totalCandidates || 0 }}</span>
            <span class="kpi-label">Total Candidates</span>
          </div>
        </div>

        <div class="kpi-card">
          <div class="kpi-icon">📄</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ stats()?.uploadsToday || 0 }}</span>
            <span class="kpi-label">Uploads Today</span>
          </div>
        </div>

        <div class="kpi-card">
          <div class="kpi-icon">✅</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ stats()?.completedToday || 0 }}</span>
            <span class="kpi-label">Completed Today</span>
          </div>
        </div>

        <div class="kpi-card">
          <div class="kpi-icon">📈</div>
          <div class="kpi-content">
            <span class="kpi-value">{{ kpi()?.averageSlaCompliance || 0 }}%</span>
            <span class="kpi-label">SLA Compliance</span>
          </div>
        </div>
      </section>

      <section class="charts-section">
        <div class="chart-card">
          <h3>Screening Trends (30 Days)</h3>
          <div class="chart-container">
            @if (loading()) {
              <div class="loading-placeholder">Loading chart...</div>
            } @else {
              <div class="bar-chart">
                @for (metric of chartData(); track $index) {
                  <div class="bar-group">
                    <div class="bar-wrapper">
                      <div class="bar created" [style.height.%]="getBarHeight(metric.created)">
                        <span class="bar-value">{{ metric.created }}</span>
                      </div>
                      <div class="bar completed" [style.height.%]="getBarHeight(metric.completed)">
                        <span class="bar-value">{{ metric.completed }}</span>
                      </div>
                    </div>
                    <span class="bar-label">{{ formatDate(metric.date) }}</span>
                  </div>
                }
              </div>
              <div class="chart-legend">
                <span class="legend-item"><span class="legend-color created"></span> Created</span>
                <span class="legend-item"><span class="legend-color completed"></span> Completed</span>
              </div>
            }
          </div>
        </div>

        <div class="chart-card">
          <h3>Quick Stats</h3>
          <div class="quick-stats">
            <div class="stat-row">
              <span class="stat-label">Total Screenings</span>
              <span class="stat-value">{{ kpi()?.totalScreenings || 0 }}</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Pending</span>
              <span class="stat-value">{{ kpi()?.pendingScreenings || 0 }}</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Completed</span>
              <span class="stat-value">{{ kpi()?.completedScreenings || 0 }}</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Avg Processing Time</span>
              <span class="stat-value">{{ kpi()?.averageCompletionTimeHours || 0 }}h</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Documents Processed</span>
              <span class="stat-value">{{ kpi()?.totalDocumentsProcessed || 0 }}</span>
            </div>
            <div class="stat-row">
              <span class="stat-label">Growth Rate</span>
              <span class="stat-value">{{ kpi()?.screeningGrowthRate || 0 }}%</span>
            </div>
          </div>
        </div>
      </section>

      <section class="activity-section">
        <h3>Recent Activity</h3>
        <div class="activity-list">
          <div class="activity-item">
            <div class="activity-icon">📋</div>
            <div class="activity-content">
              <strong>{{ stats()?.activeScreenings || 0 }} Active Screenings</strong>
              <span class="activity-meta">Currently in progress</span>
            </div>
          </div>
          <div class="activity-item">
            <div class="activity-icon">⏳</div>
            <div class="activity-content">
              <strong>{{ stats()?.pendingVerification || 0 }} Pending Verifications</strong>
              <span class="activity-meta">Awaiting action</span>
            </div>
          </div>
          <div class="activity-item">
            <div class="activity-icon">📤</div>
            <div class="activity-content">
              <strong>{{ stats()?.uploadsToday || 0 }} Documents Uploaded</strong>
              <span class="activity-meta">Today</span>
            </div>
          </div>
          <div class="activity-item">
            <div class="activity-icon">🎯</div>
            <div class="activity-content">
              <strong>{{ kpi()?.completionRate || 0 }}% Success Rate</strong>
              <span class="activity-meta">Overall completion</span>
            </div>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .dashboard {
      max-width: 1400px;
      margin: 0 auto;
      padding: 24px;
    }
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 32px;
    }
    .header-content h1 {
      font-size: 28px;
      font-weight: 600;
      margin: 0 0 8px 0;
    }
    .subtitle {
      color: #666;
      margin: 0;
    }
    .header-actions {
      display: flex;
      gap: 12px;
    }
    .btn-primary, .btn-secondary {
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
    .btn-secondary {
      background: white;
      color: #374151;
      border: 1px solid #d1d5db;
    }
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 32px;
    }
    .kpi-card {
      background: white;
      border-radius: 8px;
      padding: 20px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .kpi-card.primary { border-left: 4px solid #3b82f6; }
    .kpi-card.success { border-left: 4px solid #10b981; }
    .kpi-card.info { border-left: 4px solid #06b6d4; }
    .kpi-card.warning { border-left: 4px solid #f59e0b; }
    .kpi-icon {
      font-size: 24px;
    }
    .kpi-content {
      display: flex;
      flex-direction: column;
    }
    .kpi-value {
      font-size: 28px;
      font-weight: 600;
      color: #1a1a1a;
    }
    .kpi-label {
      font-size: 13px;
      color: #6b7280;
    }
    .kpi-trend {
      font-size: 12px;
      color: #6b7280;
    }
    .kpi-trend.up { color: #10b981; }
    .kpi-trend.down { color: #ef4444; }
    .kpi-meta {
      font-size: 12px;
      color: #6b7280;
    }
    .charts-section {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 24px;
      margin-bottom: 32px;
    }
    .chart-card {
      background: white;
      border-radius: 8px;
      padding: 24px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    .chart-card h3 {
      font-size: 16px;
      font-weight: 600;
      margin: 0 0 20px 0;
    }
    .chart-container {
      min-height: 250px;
    }
    .bar-chart {
      display: flex;
      gap: 8px;
      align-items: flex-end;
      height: 200px;
      padding-bottom: 30px;
    }
    .bar-group {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
    }
    .bar-wrapper {
      display: flex;
      gap: 4px;
      align-items: flex-end;
      height: 170px;
    }
    .bar {
      width: 16px;
      border-radius: 4px 4px 0 0;
      display: flex;
      justify-content: center;
      align-items: flex-start;
      padding-top: 4px;
      min-height: 20px;
    }
    .bar.created { background: #3b82f6; }
    .bar.completed { background: #10b981; }
    .bar-value {
      font-size: 10px;
      color: white;
      white-space: nowrap;
    }
    .bar-label {
      font-size: 10px;
      color: #6b7280;
      margin-top: 8px;
    }
    .chart-legend {
      display: flex;
      gap: 16px;
      justify-content: center;
      margin-top: 12px;
    }
    .legend-item {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 12px;
      color: #6b7280;
    }
    .legend-color {
      width: 12px;
      height: 12px;
      border-radius: 2px;
    }
    .legend-color.created { background: #3b82f6; }
    .legend-color.completed { background: #10b981; }
    .quick-stats {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    .stat-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-bottom: 12px;
      border-bottom: 1px solid #f3f4f6;
    }
    .stat-row:last-child {
      border-bottom: none;
    }
    .stat-label {
      font-size: 14px;
      color: #6b7280;
    }
    .stat-value {
      font-size: 16px;
      font-weight: 600;
      color: #1a1a1a;
    }
    .activity-section {
      background: white;
      border-radius: 8px;
      padding: 24px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    .activity-section h3 {
      font-size: 16px;
      font-weight: 600;
      margin: 0 0 20px 0;
    }
    .activity-list {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 16px;
    }
    .activity-item {
      display: flex;
      gap: 12px;
      padding: 16px;
      background: #f9fafb;
      border-radius: 8px;
    }
    .activity-icon {
      font-size: 24px;
    }
    .activity-content {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .activity-content strong {
      font-size: 14px;
      font-weight: 500;
    }
    .activity-meta {
      font-size: 12px;
      color: #6b7280;
    }
    .loading-placeholder {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 200px;
      color: #6b7280;
    }
  `]
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
