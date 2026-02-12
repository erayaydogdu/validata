import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  styles: [],
  template: `
    <div>
      <!-- Page header -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p class="mt-1 text-sm text-gray-500">Welcome back!</p>
      </div>

      <!-- Stats grid -->
      <div class="mb-8 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <div class="rounded-xl border border-gray-200 bg-white p-6">
          <h3 class="text-sm font-medium text-gray-500">Active Screenings</h3>
          <p class="mt-2 text-3xl font-bold text-gray-900">0</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white p-6">
          <h3 class="text-sm font-medium text-gray-500">Completed This Week</h3>
          <p class="mt-2 text-3xl font-bold text-gray-900">0</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white p-6">
          <h3 class="text-sm font-medium text-gray-500">Pending Documents</h3>
          <p class="mt-2 text-3xl font-bold text-gray-900">0</p>
        </div>
        <div class="rounded-xl border border-gray-200 bg-white p-6">
          <h3 class="text-sm font-medium text-gray-500">Average Time</h3>
          <p class="mt-2 text-3xl font-bold text-gray-900">0 days</p>
        </div>
      </div>

      <!-- Recent activity -->
      <div class="rounded-xl border border-gray-200 bg-white p-6">
        <h2 class="mb-4 text-lg font-semibold text-gray-900">Recent Activity</h2>
        <p class="py-8 text-center text-sm text-gray-400">No recent activity</p>
      </div>
    </div>
  `
})
export class DashboardComponent {}
