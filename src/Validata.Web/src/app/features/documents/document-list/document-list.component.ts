import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DocumentService, Document } from '../../../core/services/document.service';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="documents-page">
      <header class="page-header">
        <h1>Documents</h1>
        <button class="btn btn-primary" routerLink="/documents/upload">
          Upload Document
        </button>
      </header>

      @if (loading()) {
        <div class="loading">Loading documents...</div>
      } @else if (error()) {
        <div class="error">{{ error() }}</div>
      } @else {
        <div class="documents-grid">
          @for (doc of documents(); track doc.id) {
            <div class="document-card" [class]="'status-' + doc.status.toLowerCase()">
              <div class="doc-icon">
                <span class="icon">📄</span>
              </div>
              <div class="doc-info">
                <h3>{{ doc.fileName }}</h3>
                <p class="doc-type">{{ doc.typeName }}</p>
                <span class="status-badge" [class]="doc.status.toLowerCase()">
                  {{ doc.status }}
                </span>
                <p class="doc-date">{{ doc.createdAt | date:'medium' }}</p>
              </div>
              <div class="doc-actions">
                <button class="btn btn-sm" [routerLink]="['/documents', doc.id]">
                  View
                </button>
                <button class="btn btn-sm btn-download" (click)="download(doc.id)">
                  Download
                </button>
              </div>
            </div>
          } @empty {
            <div class="empty-state">
              <p>No documents found</p>
              <button class="btn btn-primary" routerLink="/documents/upload">
                Upload your first document
              </button>
            </div>
          }
        </div>

        @if (pagination().hasNextPage || pagination().hasPreviousPage) {
          <div class="pagination">
            <button 
              [disabled]="!pagination().hasPreviousPage"
              (click)="changePage(pagination().page - 1)">
              Previous
            </button>
            <span>Page {{ pagination().page }} of {{ pagination().totalPages }}</span>
            <button 
              [disabled]="!pagination().hasNextPage"
              (click)="changePage(pagination().page + 1)">
              Next
            </button>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .documents-page {
      padding: 2rem;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    .page-header h1 {
      margin: 0;
      font-size: 1.75rem;
    }
    .documents-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1.5rem;
    }
    .document-card {
      background: white;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .doc-icon {
      font-size: 2rem;
    }
    .doc-info h3 {
      margin: 0 0 0.5rem;
      font-size: 1rem;
    }
    .doc-type {
      color: #666;
      font-size: 0.875rem;
      margin: 0 0 0.5rem;
    }
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.75rem;
      border-radius: 999px;
      font-size: 0.75rem;
      font-weight: 500;
    }
    .status-badge.uploaded { background: #e3f2fd; color: #1976d2; }
    .status-badge.processing { background: #fff3e0; color: #f57c00; }
    .status-badge.pendingreview { background: #fce4ec; color: #c2185b; }
    .status-badge.verified { background: #e8f5e9; color: #388e3c; }
    .status-badge.rejected { background: #ffebee; color: #d32f2f; }
    .doc-date {
      font-size: 0.75rem;
      color: #999;
      margin: 0;
    }
    .doc-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: auto;
    }
    .btn {
      padding: 0.5rem 1rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.875rem;
    }
    .btn-primary {
      background: #1976d2;
      color: white;
    }
    .btn-sm {
      padding: 0.375rem 0.75rem;
      font-size: 0.75rem;
    }
    .btn-download {
      background: #f5f5f5;
      color: #333;
    }
    .pagination {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 1rem;
      margin-top: 2rem;
    }
    .pagination button {
      padding: 0.5rem 1rem;
      border: 1px solid #ddd;
      background: white;
      border-radius: 4px;
      cursor: pointer;
    }
    .pagination button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
    .empty-state {
      text-align: center;
      padding: 3rem;
      color: #666;
    }
    .loading, .error {
      text-align: center;
      padding: 2rem;
    }
    .error {
      color: #d32f2f;
    }
  `]
})
export class DocumentListComponent {
  private documentService = inject(DocumentService);
  
  documents = signal<Document[]>([]);
  pagination = signal({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });
  loading = signal(true);
  error = signal<string | null>(null);

  constructor() {
    this.loadDocuments();
  }

  loadDocuments(page = 1): void {
    this.loading.set(true);
    this.documentService.getDocuments({ page, pageSize: 20 })
      .subscribe({
        next: (response) => {
          this.documents.set(response.data);
          this.pagination.set(response.pagination);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load documents');
          this.loading.set(false);
        }
      });
  }

  changePage(page: number): void {
    this.loadDocuments(page);
  }

  download(id: string): void {
    this.documentService.downloadDocument(id)
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = '';
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: (err: unknown) => {
          console.error('Download failed', err);
        }
      });
  }
}
