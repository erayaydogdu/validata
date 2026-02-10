import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportingService, SlaReport, VerificationBreakdown } from '../../core/services/reporting.service';

@Component({
  selector: 'app-sla-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="sla-reports">
      <header class="page-header">
        <div class="header-content">
          <h1>SLA & Compliance Reports</h1>
          <p class="subtitle">Monitor service level agreements and compliance metrics</p>
        </div>
        <div class="header-actions">
          <select [(ngModel)]="selectedPeriod" (change)="loadReports()" class="period-select">
            <option value="7">Last 7 Days</option>
            <option value="30">Last 30 Days</option>
            <option value="90">Last 90 Days</option>
            <option value="365">Last Year</option>
          </select>
          <button class="btn-secondary" (click)="loadReports()">Refresh</button>
          <button class="btn-primary" (click)="exportReport()">Export CSV</button>
        </div>
      </header>

      @if (loading()) {
        <div class="loading">Loading reports...</div>
      } @else {
        <section class="sla-overview">
          <div class="overview-card main">
            <div class="compliance-gauge">
              <svg viewBox="0 0 120 120">
                <circle cx="60" cy="60" r="50" fill="none" stroke="#e5e7eb" stroke-width="10"/>
                <circle cx="60" cy="60" r="50" fill="none" stroke="#10b981" stroke-width="10"
                        [attr.stroke-dasharray]="getGaugeValues(sla()?.slaComplianceRate || 0)"
                        stroke-linecap="round"
                        transform="rotate(-90 60 60)"/>
              </svg>
              <div class="gauge-text">
                <span class="gauge-value">{{ sla()?.slaComplianceRate || 0 }}%</span>
                <span class="gauge-label">SLA Compliance</span>
              </div>
            </div>
          </div>

          <div class="overview-card">
            <div class="metric-header">Completed</div>
            <div class="metric-value success">{{ sla()?.completed || 0 }}</div>
            <div class="metric-detail">{{ sla()?.onTime || 0 }} on-time</div>
          </div>

          <div class="overview-card">
            <div class="metric-header">Overdue</div>
            <div class="metric-value danger">{{ sla()?.overdue || 0 }}</div>
            <div class="metric-detail">{{ getOverduePercentage() }}% of completed</div>
          </div>

          <div class="overview-card">
            <div class="metric-header">Avg. Completion</div>
            <div class="metric-value">{{ sla()?.averageDaysToComplete || 0 }}d</div>
            <div class="metric-detail">Target: {{ sla()?.targetCompletionDays || 0 }}d</div>
          </div>

          <div class="overview-card">
            <div class="metric-header">Projected Rate</div>
            <div class="metric-value info">{{ sla()?.projectedOnTimeRate || 0 }}%</div>
            <div class="metric-detail">Based on current pace</div>
          </div>
        </section>

        <section class="breakdown-section">
          <div class="breakdown-card">
            <h3>Verification Breakdown</h3>
            <div class="breakdown-bars">
              <div class="breakdown-row">
                <span class="breakdown-label">Completed</span>
                <div class="breakdown-bar">
                  <div class="bar-fill success" [style.width.%]="getBreakdownPercentage('completed')"></div>
                </div>
                <span class="breakdown-value">{{ verification()?.completed || 0 }}</span>
              </div>
              <div class="breakdown-row">
                <span class="breakdown-label">In Progress</span>
                <div class="breakdown-bar">
                  <div class="bar-fill info" [style.width.%]="getBreakdownPercentage('inProgress')"></div>
                </div>
                <span class="breakdown-value">{{ verification()?.inProgress || 0 }}</span>
              </div>
              <div class="breakdown-row">
                <span class="breakdown-label">Pending</span>
                <div class="breakdown-bar">
                  <div class="bar-fill warning" [style.width.%]="getBreakdownPercentage('pending')"></div>
                </div>
                <span class="breakdown-value">{{ verification()?.pending || 0 }}</span>
              </div>
              <div class="breakdown-row">
                <span class="breakdown-label">Failed</span>
                <div class="breakdown-bar">
                  <div class="bar-fill danger" [style.width.%]="getBreakdownPercentage('failed')"></div>
                </div>
                <span class="breakdown-value">{{ verification()?.failed || 0 }}</span>
              </div>
            </div>
            <div class="completion-rate">
              <span>Completion Rate:</span>
              <strong>{{ verification()?.completionRate || 0 }}%</strong>
            </div>
          </div>

          <div class="breakdown-card">
            <h3>By Priority</h3>
            <table class="priority-table">
              <thead>
                <tr>
                  <th>Priority</th>
                  <th>Total</th>
                  <th>Completed</th>
                  <th>Avg Days</th>
                </tr>
              </thead>
              <tbody>
                @for (item of sla()?.breakdownByPriority || []; track item.priority) {
                  <tr>
                    <td>
                      <span class="priority-badge" [class]="getPriorityClass(item.priority)">
                        {{ item.priorityName }}
                      </span>
                    </td>
                    <td>{{ item.total }}</td>
                    <td>{{ item.completed }}</td>
                    <td>{{ item.averageDays | number:'1.1-1' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <div class="breakdown-card">
            <h3>By Verification Type</h3>
            <div class="type-list">
              @for (type of verification()?.byType || []; track type.typeValue) {
                <div class="type-item">
                  <div class="type-header">
                    <span class="type-name">{{ type.type }}</span>
                    <span class="type-rate">{{ type.completed }}/{{ type.total }}</span>
                  </div>
                  <div class="type-bar">
                    <div class="bar-fill primary" [style.width.%]="(type.completed / type.total) * 100"></div>
                  </div>
                  <div class="type-meta">
                    <span>Avg: {{ type.averageDurationHours | number:'1.1-1' }}h</span>
                    <span>{{ type.failed }} failed</span>
                  </div>
                </div>
              }
            </div>
          </div>
        </section>
      }
    </div>
  `,
  styles: [`
    .sla-reports {
      max-width: 1400px;
      margin: 0 auto;
      padding: 24px;
    }
    .page-header {
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
    .period-select {
      padding: 10px 16px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 14px;
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
    .sla-overview {
      display: grid;
      grid-template-columns: 300px repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 32px;
    }
    .overview-card {
      background: white;
      border-radius: 8px;
      padding: 24px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
      text-align: center;
    }
    .overview-card.main {
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .compliance-gauge {
      position: relative;
      width: 150px;
      height: 150px;
    }
    .compliance-gauge svg {
      width: 100%;
      height: 100%;
    }
    .gauge-text {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      text-align: center;
    }
    .gauge-value {
      display: block;
      font-size: 28px;
      font-weight: 600;
      color: #1a1a1a;
    }
    .gauge-label {
      font-size: 12px;
      color: #6b7280;
    }
    .metric-header {
      font-size: 13px;
      color: #6b7280;
      margin-bottom: 8px;
    }
    .metric-value {
      font-size: 32px;
      font-weight: 600;
      color: #1a1a1a;
    }
    .metric-value.success { color: #10b981; }
    .metric-value.danger { color: #ef4444; }
    .metric-value.info { color: #3b82f6; }
    .metric-detail {
      font-size: 13px;
      color: #6b7280;
      margin-top: 4px;
    }
    .breakdown-section {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 24px;
    }
    .breakdown-card {
      background: white;
      border-radius: 8px;
      padding: 24px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
    .breakdown-card h3 {
      font-size: 16px;
      font-weight: 600;
      margin: 0 0 20px 0;
    }
    .breakdown-row {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 12px;
    }
    .breakdown-label {
      width: 100px;
      font-size: 13px;
      color: #6b7280;
    }
    .breakdown-bar {
      flex: 1;
      height: 8px;
      background: #f3f4f6;
      border-radius: 4px;
      overflow: hidden;
    }
    .bar-fill {
      height: 100%;
      border-radius: 4px;
      transition: width 0.3s;
    }
    .bar-fill.success { background: #10b981; }
    .bar-fill.info { background: #3b82f6; }
    .bar-fill.warning { background: #f59e0b; }
    .bar-fill.danger { background: #ef4444; }
    .bar-fill.primary { background: #6366f1; }
    .breakdown-value {
      width: 40px;
      font-size: 13px;
      font-weight: 500;
      text-align: right;
    }
    .completion-rate {
      display: flex;
      justify-content: space-between;
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid #e5e7eb;
      font-size: 14px;
    }
    .completion-rate strong {
      color: #10b981;
    }
    .priority-table {
      width: 100%;
      border-collapse: collapse;
    }
    .priority-table th,
    .priority-table td {
      padding: 10px 8px;
      text-align: left;
      border-bottom: 1px solid #e5e7eb;
      font-size: 13px;
    }
    .priority-table th {
      font-weight: 600;
      color: #6b7280;
      text-transform: uppercase;
      font-size: 11px;
    }
    .priority-badge {
      display: inline-block;
      padding: 3px 8px;
      border-radius: 9999px;
      font-size: 11px;
      font-weight: 500;
    }
    .priority-badge.high { background: #fee2e2; color: #991b1b; }
    .priority-badge.normal { background: #dbeafe; color: #1e40af; }
    .priority-badge.low { background: #d1fae5; color: #065f46; }
    .type-list {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    .type-item {
      padding-bottom: 16px;
      border-bottom: 1px solid #f3f4f6;
    }
    .type-item:last-child {
      border-bottom: none;
      padding-bottom: 0;
    }
    .type-header {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
    }
    .type-name {
      font-size: 13px;
      font-weight: 500;
    }
    .type-rate {
      font-size: 12px;
      color: #6b7280;
    }
    .type-bar {
      height: 6px;
      background: #f3f4f6;
      border-radius: 3px;
      overflow: hidden;
      margin-bottom: 8px;
    }
    .type-meta {
      display: flex;
      justify-content: space-between;
      font-size: 11px;
      color: #9ca3af;
    }
    .loading {
      text-align: center;
      padding: 60px;
      color: #6b7280;
    }
  `]
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

  getPriorityClass(priority: number): string {
    const classes: Record<number, string> = { 1: 'low', 2: 'normal', 3: 'high', 4: 'high' };
    return classes[priority] || 'normal';
  }
}
