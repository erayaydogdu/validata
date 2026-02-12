import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { DocumentService, Document } from '../../../core/services/document.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  styles: [],
  template: `
    <div class="mx-auto max-w-2xl">
      <!-- Header -->
      <div class="mb-8 flex items-center justify-between">
        <h1 class="text-2xl font-bold text-gray-900">Upload Document</h1>
        <a
          routerLink="/documents"
          class="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50">
          Back to Documents
        </a>
      </div>

      <!-- Upload card -->
      <div class="rounded-xl border border-gray-200 bg-white p-6 sm:p-8">
        <form (ngSubmit)="onSubmit()">
          <div class="mb-6">
            <label class="mb-1.5 block text-sm font-medium text-gray-700">Document Type</label>
            <select
              [(ngModel)]="documentType"
              name="documentType"
              required
              class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none">
              <option value="">Select type...</option>
              <option [value]="1">Passport</option>
              <option [value]="2">ID Card</option>
              <option [value]="3">Driver's License</option>
              <option [value]="4">Diploma</option>
              <option [value]="5">Certificate</option>
              <option [value]="6">Work History</option>
              <option [value]="7">Tax Document</option>
              <option [value]="8">Criminal Record</option>
              <option [value]="9">Reference Letter</option>
              <option [value]="10">Other</option>
            </select>
          </div>

          <div class="mb-6">
            <label class="mb-1.5 block text-sm font-medium text-gray-700">File</label>
            <div class="relative rounded-lg border-2 border-dashed border-gray-300 p-6 text-center transition hover:border-brand-400">
              <svg class="mx-auto mb-2 h-8 w-8 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M3 16.5v2.25A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75V16.5m-13.5-9L12 3m0 0 4.5 4.5M12 3v13.5"/>
              </svg>
              <p class="mb-1 text-sm text-gray-600">Click to select or drag and drop</p>
              <p class="text-xs text-gray-400">PDF, JPG, PNG, DOC up to 10MB</p>
              <input
                type="file"
                (change)="onFileSelect($event)"
                accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                required
                class="absolute inset-0 cursor-pointer opacity-0">
            </div>
          </div>

          @if (selectedFile) {
            <div class="mb-4 flex items-center gap-2 rounded-lg bg-gray-50 px-4 py-3">
              <span>📎</span>
              <span class="text-sm font-medium text-gray-700">{{ selectedFile.name }}</span>
              <span class="text-xs text-gray-400">({{ formatFileSize(selectedFile.size) }})</span>
            </div>
          }

          @if (uploading()) {
            <div class="mb-4">
              <div class="h-2 overflow-hidden rounded-full bg-gray-200">
                <div class="h-full rounded-full bg-brand-500 transition-all duration-300" [style.width.%]="uploadProgress()"></div>
              </div>
              <p class="mt-1.5 text-center text-xs text-gray-500">Uploading... {{ uploadProgress() }}%</p>
            </div>
          }

          @if (error()) {
            <div class="mb-4 rounded-lg bg-danger-50 px-4 py-3 text-sm text-danger-700">{{ error() }}</div>
          }

          <div class="flex justify-end">
            <button
              type="submit"
              class="rounded-lg bg-brand-500 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-brand-600 disabled:cursor-not-allowed disabled:opacity-50"
              [disabled]="!canSubmit() || uploading()">
              Upload Document
            </button>
          </div>
        </form>
      </div>
    </div>
  `
})
export class DocumentUploadComponent {
  private documentService = inject(DocumentService);
  private router = inject(Router);

  documentType = '';
  selectedFile: File | null = null;
  uploading = signal(false);
  uploadProgress = signal(0);
  error = signal<string | null>(null);

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

  canSubmit(): boolean {
    return !!this.selectedFile && !!this.documentType;
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  onSubmit(): void {
    if (!this.canSubmit()) return;

    this.uploading.set(true);
    this.uploadProgress.set(0);
    this.error.set(null);

    this.documentService.uploadDocument(
      this.selectedFile!,
      parseInt(this.documentType)
    ).subscribe({
      next: (doc: Document) => {
        this.uploading.set(false);
        this.router.navigate(['/documents', doc.id]);
      },
      error: (err: any) => {
        this.error.set(err.error?.message || 'Upload failed');
        this.uploading.set(false);
      }
    });
  }
}
