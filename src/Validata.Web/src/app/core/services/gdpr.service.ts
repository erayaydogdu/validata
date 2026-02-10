import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface GDPRRequest {
  id: string;
  requestType: number;
  requestTypeName: string;
  status: number;
  statusName: string;
  requestedAt: Date;
  completedAt?: Date;
  estimatedCompletionDays: number;
  willReceiveEmail: boolean;
  downloadUrl?: string;
}

export interface CreateGDPRRequest {
  requestType: number;
  candidateId: string;
  reason?: string;
}

export interface Consent {
  id: string;
  candidateId: string;
  consentType: string;
  granted: boolean;
  grantedAt: Date;
  details?: string;
}

interface ConsentRecord {
  consentType: string;
  granted: boolean;
  grantedAt: Date;
  details?: string;
}

@Injectable({
  providedIn: 'root'
})
export class GDPRService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/gdpr`;

  createRequest(request: CreateGDPRRequest): Observable<GDPRRequest> {
    return this.http.post<GDPRRequest>(`${this.baseUrl}/requests`, request);
  }

  getRequest(id: string): Observable<GDPRRequest> {
    return this.http.get<GDPRRequest>(`${this.baseUrl}/requests/${id}`);
  }

  getRequests(filters?: { status?: number; candidateId?: string; page?: number; pageSize?: number }): Observable<{
    data: GDPRRequest[];
    totalCount: number;
    page: number;
    pageSize: number;
  }> {
    let params = new HttpParams();
    if (filters?.status) params = params.set('status', filters.status.toString());
    if (filters?.candidateId) params = params.set('candidateId', filters.candidateId);
    if (filters?.page) params = params.set('page', filters.page.toString());
    if (filters?.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    return this.http.get<any>(`${this.baseUrl}/requests`, { params });
  }

  processRequest(id: string): Observable<GDPRRequest> {
    return this.http.post<GDPRRequest>(`${this.baseUrl}/requests/${id}/process`, {});
  }

  completeRequest(id: string, result: string): Observable<GDPRRequest> {
    return this.http.post<GDPRRequest>(`${this.baseUrl}/requests/${id}/complete`, { result });
  }

  rejectRequest(id: string, reason: string): Observable<GDPRRequest> {
    return this.http.post<GDPRRequest>(`${this.baseUrl}/requests/${id}/reject`, { reason });
  }

  exportCandidateData(candidateId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export/${candidateId}`, { responseType: 'blob' });
  }

  deleteCandidateData(candidateId: string): Observable<{ message: string }> {
    return this.http.delete<any>(`${this.baseUrl}/candidates/${candidateId}`);
  }

  recordConsent(consent: { candidateId: string; consentType: string; granted: boolean; details?: string }): Observable<{ message: string; consentId: string }> {
    return this.http.post<any>(`${this.baseUrl}/consent`, consent);
  }

  getConsents(candidateId: string): Observable<Consent[]> {
    return this.http.get<Consent[]>(`${this.baseUrl}/consent/${candidateId}`);
  }
}
