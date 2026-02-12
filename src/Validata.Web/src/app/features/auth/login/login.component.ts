import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="auth-page">
      <div class="auth-container">
        <h1>Login to Validata</h1>
        <form (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Email</label>
            <input type="email" [(ngModel)]="email" name="email" required>
          </div>
          <div class="form-group">
            <label>Password</label>
            <input type="password" [(ngModel)]="password" name="password" required>
          </div>
          @if (error()) {
            <div class="error">{{ error() }}</div>
          }
          <button type="submit" class="btn btn-primary" [disabled]="loading()">
            {{ loading() ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>
        <p class="auth-link">
          Don't have an account? <a routerLink="/register">Register</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f5f5f5;
    }
    .auth-container {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      width: 100%;
      max-width: 400px;
    }
    h1 {
      margin: 0 0 1.5rem;
      text-align: center;
    }
    .form-group {
      margin-bottom: 1rem;
    }
    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
    }
    .form-group input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      box-sizing: border-box;
    }
    .btn {
      width: 100%;
      padding: 0.75rem;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 1rem;
    }
    .btn-primary {
      background: #1976d2;
      color: white;
    }
    .btn:disabled {
      background: #ccc;
    }
    .error {
      color: #d32f2f;
      padding: 0.75rem;
      background: #ffebee;
      border-radius: 4px;
      margin-bottom: 1rem;
    }
    .auth-link {
      text-align: center;
      margin-top: 1rem;
    }
    .auth-link a {
      color: #1976d2;
    }
  `]
})
export class LoginComponent {
  email = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);
  private authService = inject(AuthService);
  private router = inject(Router);

  onSubmit(): void {
    this.loading.set(true);
    this.error.set(null);

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Login failed. Please check your credentials.');
        this.loading.set(false);
      }
    });
  }
}
