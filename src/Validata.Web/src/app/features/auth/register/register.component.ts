import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
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
          <h1 class="text-2xl font-bold text-gray-900">Create your account</h1>
          <p class="mt-2 text-sm text-gray-500">Get started with Validata</p>
        </div>

        <!-- Card -->
        <div class="rounded-xl border border-gray-200 bg-white p-6 shadow-sm sm:p-8">
          <form (ngSubmit)="onSubmit()">
            <div class="mb-5 grid grid-cols-2 gap-4">
              <div>
                <label class="mb-1.5 block text-sm font-medium text-gray-700">First Name</label>
                <input
                  type="text"
                  [(ngModel)]="firstName"
                  name="firstName"
                  required
                  class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"
                  placeholder="Jane">
              </div>
              <div>
                <label class="mb-1.5 block text-sm font-medium text-gray-700">Last Name</label>
                <input
                  type="text"
                  [(ngModel)]="lastName"
                  name="lastName"
                  required
                  class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"
                  placeholder="Doe">
              </div>
            </div>
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
            <div class="mb-5">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Password</label>
              <input
                type="password"
                [(ngModel)]="password"
                name="password"
                required
                class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"
                placeholder="Create a password">
            </div>
            <div class="mb-6">
              <label class="mb-1.5 block text-sm font-medium text-gray-700">Organization (optional)</label>
              <input
                type="text"
                [(ngModel)]="organizationName"
                name="organizationName"
                class="block w-full rounded-lg border border-gray-300 px-3.5 py-2.5 text-sm text-gray-900 placeholder-gray-400 shadow-sm transition focus:border-brand-500 focus:ring-2 focus:ring-brand-500/20 focus:outline-none"
                placeholder="Your company name">
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
                Creating account...
              } @else {
                Create account
              }
            </button>
          </form>
        </div>

        <p class="mt-6 text-center text-sm text-gray-500">
          Already have an account?
          <a routerLink="/login" class="font-semibold text-brand-600 hover:text-brand-500">Sign in</a>
        </p>
      </div>
    </div>
  `
})
export class RegisterComponent {
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  organizationName = '';
  loading = signal(false);
  error = signal<string | null>(null);
  private authService = inject(AuthService);
  private router = inject(Router);

  onSubmit(): void {
    this.loading.set(true);
    this.error.set(null);

    this.authService.register({
      email: this.email,
      password: this.password,
      firstName: this.firstName,
      lastName: this.lastName,
      organizationName: this.organizationName || undefined
    }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Registration failed');
        this.loading.set(false);
      }
    });
  }
}
