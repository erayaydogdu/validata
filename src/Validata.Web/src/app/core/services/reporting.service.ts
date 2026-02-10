import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface KpiSummary {
  totalScreenings: number;
  pendingScreenings: number;
  completedScreenings: number;
  inProgressScreenings: number;
  averageCompletionTimeHours: number;
  screeningGrowthRate: number;
  completionRate: number;
  totalDocumentsProcessed: number;
  averageSlaCompliance: number;
  weeklyChange: number;
}

export interface DashboardStats {
  totalCandidates: number;
  activeScreenings: number;
  pendingVerification: number;
  completedToday: number;
  completedThisWeek: number;
  averageTurnaroundDays: number;
  pendingActions: number;
  uploadsToday: number;
}

export interface ScreeningTrends {
  period: string;
  dailyMetrics: DailyMetric[];
  totalCreated: number;
  totalCompleted: number;
  averageDailyCreated: number;
  averageDailyCompleted: number;
  peakDay?: Date;
  growthTrend: string;
}

export interface DailyMetric {
  date: Date;
  created: number;
  completed: number;
  inProgress: number;
}

export interface VerificationBreakdown {
  totalSteps: number;
  pending: number;
  inProgress: number;
  completed: number;
  failed: number;
  completionRate: number;
  byType: VerificationTypeMetric[];
}

export interface VerificationTypeMetric {
  type: string;
  typeValue: number;
  total: number;
  completed: number;
  failed: number;
  averageDurationHours: number;
}

export interface SlaReport {
  period: string;
  totalScreenings: number;
  completed: number;
  onTime: number;
  overdue: number;
  noDueDate: number;
  slaComplianceRate: number;
  averageCompletionTimeHours: number;
  averageDaysToComplete: number;
  targetCompletionDays: number;
  projectedOnTimeRate: number;
  breakdownByPriority: PriorityMetric[];
}

export interface PriorityMetric {
  priority: number;
  priorityName: string;
  total: number;
  completed: number;
  inProgress: number;
  averageDays: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReportingService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/reports`;

  getKpiSummary(fromDate?: Date, toDate?: Date): Observable<KpiSummary> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate.toISOString());
    if (toDate) params = params.set('toDate', toDate.toISOString());
    return this.http.get<KpiSummary>(`${this.baseUrl}/kpi`, { params });
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/dashboard`);
  }

  getScreeningTrends(days: number = 30): Observable<ScreeningTrends> {
    return this.http.get<ScreeningTrends>(`${this.baseUrl}/trends`, { params: { days: days.toString() } });
  }

  getVerificationBreakdown(): Observable<VerificationBreakdown> {
    return this.http.get<VerificationBreakdown>(`${this.baseUrl}/verification`);
  }

  getSlaReport(fromDate?: Date, toDate?: Date): Observable<SlaReport> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate.toISOString());
    if (toDate) params = params.set('toDate', toDate.toISOString());
    return this.http.get<SlaReport>(`${this.baseUrl}/sla`, { params });
  }

  exportKpi(fromDate?: Date, toDate?: Date): Observable<Blob> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate.toISOString());
    if (toDate) params = params.set('toDate', toDate.toISOString());
    return this.http.get(`${this.baseUrl}/export/kpi`, { params, responseType: 'blob' });
  }

  exportSla(fromDate?: Date, toDate?: Date): Observable<Blob> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate.toISOString());
    if (toDate) params = params.set('toDate', toDate.toISOString());
    return this.http.get(`${this.baseUrl}/export/sla`, { params, responseType: 'blob' });
  }

  downloadBlob(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
