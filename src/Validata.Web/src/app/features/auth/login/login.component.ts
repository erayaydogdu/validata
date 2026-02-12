import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  styles: [],
  template: `
    <div class="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12">
      <div class="w-full max-w-md">
        <!-- Brand -->
        <div class="mb-8 text-center">
          <div class="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-500">
            <svg class="h-7 w-7 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75 11.25 15 15 9.75m-3-7.036A11.959 11.959 0 0 1 3.598 6 11.99 11.99 0 0 0 3 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285Z"/>
            </svg>
          </div>
          <h1 class="text-2xl font-bold text-gray-900">Sign in to Validata</h1>
          <p class="mt-2 text-sm text-gray-500">Enter your credentials to access your account</p>
        </div>

        <!-- Card -->
        <div class="rounded-xl border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
          <form (ngSubmit)="onSubmit()">
            <div class="mb-5">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Email</label>
              <input
                type="email"
                [(ngModel)]="email"
                name="email"
                required
                class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"
                placeholder="you@example.com">
            </div>
            <div class="mb-6">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Password</label>
              <input
                type="password"
                [(ngModel)]="password"
                name="password"
                required
                class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"
                placeholder="Enter your password">
            </div>

            @if (error()) {
              <div class="mb-4 rounded-lg bg-danger-50 px-4 py-3 text-sm text-danger-700">
                {{ error() }}
              </div>
            }

            <button
              type="submit"
              class="flex w-full items-center justify-center rounded-lg bg-brand-500 px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-brand-600 focus:ring-2 focus:ring-brand-500/20 focus:outline-none disabled:cursor-not-allowed disabled:opacity-50"
              [disabled]="loading()">
              @if (loading()) {
                <svg class="mr-2 h-4 w-4 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Signing in...
              } @else {
                Sign in
              }
            </button>
          </form>
        </div>

        <p class="mt-6 text-center text-sm text-gray-500">
          Don't have an account?
          <a routerLink="/register" class="font-semibold text-brand-600 hover:text-brand-500">Create one</a>
        </p>

        <!-- Test accounts hint -->
        <div class="mt-6 rounded-xl border border-dashed border-gray-300 bg-white p-4">
          <div class="mb-3 flex items-center gap-2">
            <svg class="h-4 w-4 text-amber-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9.75 3.104v5.714a2.25 2.25 0 0 1-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 0 1 4.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19.8 15.3M14.25 3.104c.251.023.501.05.75.082M19.8 15.3l-1.57.393A9.065 9.065 0 0 1 12 15a9.065 9.065 0 0 0-6.23.693L5 14.5m14.8.8 1.402 1.402c1.232 1.232.65 3.318-1.067 3.611A48.309 48.309 0 0 1 12 21c-2.773 0-5.491-.235-8.135-.687-1.718-.293-2.3-2.379-1.067-3.61L5 14.5"/>
            </svg>
            <span class="text-xs font-semibold text-gray-700">Test Accounts</span>
            <span class="rounded-full bg-amber-50 px-1.5 py-0.5 text-[10px] font-medium text-amber-600">DEV</span>
          </div>
          <p class="mb-2 text-[11px] text-gray-400">Password for all: <code class="rounded bg-gray-100 px-1 py-0.5 font-mono text-gray-600">Password123!</code></p>
          <div class="space-y-1">
            @for (account of testAccounts; track account.email) {
              <button
                type="button"
                class="flex w-full items-center justify-between rounded-md px-2 py-1 text-left text-xs transition hover:bg-gray-50"
                (click)="fillTestAccount(account.email)">
                <span class="font-medium text-gray-700">{{ account.email }}</span>
                <span class="rounded-full bg-gray-100 px-2 py-0.5 text-[10px] text-gray-500">{{ account.role }}</span>
              </button>
            }
          </div>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);
  private authService = inject(AuthService);
  private router = inject(Router);

  testAccounts = [
    { email: 'admin@validata.com', role: 'Admin' },
    { email: 'hr.manager@acme.com', role: 'HR Manager' },
    { email: 'hr.staff@acme.com', role: 'HR Staff' },
    { email: 'candidate@example.com', role: 'Candidate' },
    { email: 'verifier@validata.com', role: 'Verifier' },
    { email: 'compliance@validata.com', role: 'Compliance' },
  ];

  fillTestAccount(email: string): void {
    this.email = email;
    this.password = 'Password123!';
  }

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
