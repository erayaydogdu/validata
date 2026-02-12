import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DocumentService, Document } from '../../../core/services/document.service';

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  styles: [],
  template: `
    <div>
      <!-- Header -->
      <div class="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 class="text-2xl font-bold text-gray-900">Documents</h1>
        <a
          routerLink="/documents/upload"
          class="inline-flex items-center justify-center rounded-lg bg-brand-500 px-4 py-2.5 text-sm font-medium text-white shadow-sm transition hover:bg-brand-600">
          <svg class="mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15"/>
          </svg>
          Upload Document
        </a>
      </div>

      @if (loading()) {
        <div class="py-12 text-center text-sm text-gray-400">Loading documents...</div>
      } @else if (error()) {
        <div class="rounded-lg bg-danger-50 px-4 py-3 text-sm text-danger-700">{{ error() }}</div>
      } @else {
        <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
          @for (doc of documents(); track doc.id) {
            <div class="rounded-xl border border-gray-200 bg-white p-5 transition hover:shadow-md">
              <div class="mb-3 flex items-start justify-between">
                <span class="text-2xl">📄</span>
                <span class="rounded-full px-2.5 py-0.5 text-xs font-medium"
                  [class]="getStatusClasses(doc.status)">
                  {{ doc.status }}
                </span>
              </div>
              <h3 class="mb-1 truncate text-sm font-semibold text-gray-900">{{ doc.fileName }}</h3>
              <p class="mb-3 text-xs text-gray-500">{{ doc.typeName }}</p>
              <p class="mb-4 text-xs text-gray-400">{{ doc.createdAt | date:'medium' }}</p>
              <div class="flex gap-2">
                <button
                  class="rounded-lg border border-gray-300 px-3 py-1.5 text-xs font-medium text-gray-700 transition hover:bg-gray-50"
                  [routerLink]="['/documents', doc.id]">
                  View
                </button>
                <button
                  class="rounded-lg bg-gray-100 px-3 py-1.5 text-xs font-medium text-gray-600 transition hover:bg-gray-200"
                  (click)="download(doc.id)">
                  Download
                </button>
              </div>
            </div>
          } @empty {
            <div class="col-span-full py-12 text-center">
              <p class="mb-4 text-sm text-gray-400">No documents found</p>
              <a
                routerLink="/documents/upload"
                class="inline-flex items-center rounded-lg bg-brand-500 px-4 py-2 text-sm font-medium text-white transition hover:bg-brand-600">
                Upload your first document
              </a>
            </div>
          }
        </div>

        @if (pagination().hasNextPage || pagination().hasPreviousPage) {
          <div class="mt-8 flex items-center justify-center gap-4">
            <button
              class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              [disabled]="!pagination().hasPreviousPage"
              (click)="changePage(pagination().page - 1)">
              Previous
            </button>
            <span class="text-sm text-gray-500">Page {{ pagination().page }} of {{ pagination().totalPages }}</span>
            <button
              class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              [disabled]="!pagination().hasNextPage"
              (click)="changePage(pagination().page + 1)">
              Next
            </button>
          </div>
        }
      }
    </div>
  `
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

  getStatusClasses(status: string): string {
    const s = status.toLowerCase();
    const map: Record<string, string> = {
      uploaded: 'bg-blue-50 text-blue-700',
      processing: 'bg-amber-50 text-amber-700',
      pendingreview: 'bg-pink-50 text-pink-700',
      verified: 'bg-green-50 text-green-700',
      rejected: 'bg-red-50 text-red-700'
    };
    return map[s] || 'bg-gray-50 text-gray-700';
  }
}
