import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { DocumentService, Document } from '../../../core/services/document.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-document-upload',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="upload-page">
      <header class="page-header">
        <h1>Upload Document</h1>
        <button class="btn btn-secondary" routerLink="/documents">
          Back to Documents
        </button>
      </header>

      <div class="upload-form">
        <form (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Document Type</label>
            <select [(ngModel)]="documentType" name="documentType" required>
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

          <div class="form-group">
            <label>File</label>
            <input 
              type="file" 
              (change)="onFileSelect($event)"
              accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
              required>
          </div>

          <div class="form-group">
            <label>Screening ID (optional)</label>
            <input 
              type="text" 
              [(ngModel)]="screeningId" 
              name="screeningId"
              placeholder="Associate with screening...">
          </div>

          <div class="form-group">
            <label>Candidate ID (optional)</label>
            <input 
              type="text" 
              [(ngModel)]="candidateId" 
              name="candidateId"
              placeholder="Associate with candidate...">
          </div>

          @if (selectedFile) {
            <div class="selected-file">
              <span class="file-icon">📎</span>
              <span>{{ selectedFile.name }}</span>
              <span class="file-size">({{ formatFileSize(selectedFile.size) }})</span>
            </div>
          }

          @if (uploading()) {
            <div class="progress-bar">
              <div class="progress" [style.width.%]="uploadProgress()"></div>
            </div>
            <p class="upload-status">Uploading... {{ uploadProgress() }}%</p>
          }

          @if (error()) {
            <div class="error-message">{{ error() }}</div>
          }

          <div class="form-actions">
            <button 
              type="submit" 
              class="btn btn-primary"
              [disabled]="!canSubmit() || uploading()">
              Upload Document
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .upload-page {
      padding: 2rem;
      max-width: 600px;
      margin: 0 auto;
    }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    .page-header h1 {
      margin: 0;
    }
    .upload-form {
      background: white;
      border-radius: 8px;
      padding: 2rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .form-group {
      margin-bottom: 1.5rem;
    }
    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: 500;
    }
    .form-group input[type="text"],
    .form-group select {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;
    }
    .form-group input[type="file"] {
      padding: 0.5rem;
      border: 1px dashed #ddd;
      border-radius: 4px;
      width: 100%;
    }
    .selected-file {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.75rem;
      background: #f5f5f5;
      border-radius: 4px;
      margin-bottom: 1rem;
    }
    .file-size {
      color: #666;
      font-size: 0.875rem;
    }
    .progress-bar {
      height: 8px;
      background: #e0e0e0;
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 0.5rem;
    }
    .progress {
      height: 100%;
      background: #1976d2;
      transition: width 0.3s;
    }
    .upload-status {
      text-align: center;
      color: #666;
      margin-bottom: 1rem;
    }
    .error-message {
      color: #d32f2f;
      padding: 0.75rem;
      background: #ffebee;
      border-radius: 4px;
      margin-bottom: 1rem;
    }
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
    }
    .btn {
      padding: 0.75rem 1.5rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
    }
    .btn-primary {
      background: #1976d2;
      color: white;
    }
    .btn-primary:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
    .btn-secondary {
      background: #f5f5f5;
      color: #333;
    }
  `]
})
export class DocumentUploadComponent {
  private documentService = inject(DocumentService);
  private router = inject(Router);

  documentType = '';
  screeningId = '';
  candidateId = '';
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
      parseInt(this.documentType),
      this.screeningId || undefined,
      this.candidateId || undefined
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
