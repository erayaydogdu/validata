import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Document {
  id: string;
  type: number;
  typeName: string;
  fileName: string;
  fileSize?: number;
  mimeType?: string;
  status: string;
  confidenceScore?: number;
  createdAt: string;
  expiresAt?: string;
}

export interface DocumentQuery {
  page?: number;
  pageSize?: number;
  screeningId?: string;
  candidateId?: string;
  type?: number;
  status?: number;
}

export interface PaginationResponse<T> {
  data: T[];
  pagination: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/documents`;

  getDocuments(query: DocumentQuery = {}): Observable<PaginationResponse<Document>> {
    let params = new HttpParams();
    if (query.page) params = params.set('page', query.page.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    if (query.screeningId) params = params.set('screeningId', query.screeningId);
    if (query.candidateId) params = params.set('candidateId', query.candidateId);
    if (query.type !== undefined) params = params.set('type', query.type.toString());
    if (query.status !== undefined) params = params.set('status', query.status.toString());

    return this.http.get<PaginationResponse<Document>>(this.apiUrl, { params });
  }

  getDocument(id: string): Observable<Document> {
    return this.http.get<Document>(`${this.apiUrl}/${id}`);
  }

  uploadDocument(file: File, type: number): Observable<Document> {
    const formData = new FormData();
    formData.append('File', file);
    formData.append('Type', type.toString());

    return this.http.post<Document>(`${this.apiUrl}/upload`, formData);
  }

  downloadDocument(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/download`, { responseType: 'blob' });
  }

  verifyDocument(id: string, notes?: string): Observable<{ id: string; status: string }> {
    return this.http.put<{ id: string; status: string }>(`${this.apiUrl}/${id}/verify`, { notes });
  }

  rejectDocument(id: string, reason: string): Observable<{ id: string; status: string }> {
    return this.http.put<{ id: string; status: string }>(`${this.apiUrl}/${id}/reject`, { reason });
  }

  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
