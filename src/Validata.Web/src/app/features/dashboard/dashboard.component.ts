import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard">
      <header class="page-header">
        <h1>Dashboard</h1>
        <div class="user-info">Welcome back!</div>
      </header>
      
      <div class="stats-grid">
        <div class="stat-card">
          <h3>Active Screenings</h3>
          <p class="stat-value">0</p>
        </div>
        <div class="stat-card">
          <h3>Completed This Week</h3>
          <p class="stat-value">0</p>
        </div>
        <div class="stat-card">
          <h3>Pending Documents</h3>
          <p class="stat-value">0</p>
        </div>
        <div class="stat-card">
          <h3>Average Time</h3>
          <p class="stat-value">0 days</p>
        </div>
      </div>

      <div class="dashboard-section">
        <h2>Recent Activity</h2>
        <p class="empty-message">No recent activity</p>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
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
    }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }
    .stat-card {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .stat-card h3 {
      margin: 0 0 0.5rem;
      color: #666;
      font-size: 0.875rem;
      font-weight: normal;
    }
    .stat-value {
      font-size: 2rem;
      font-weight: bold;
      margin: 0;
    }
    .dashboard-section {
      background: white;
      padding: 1.5rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .dashboard-section h2 {
      margin: 0 0 1rem;
    }
    .empty-message {
      color: #999;
      text-align: center;
      padding: 2rem;
    }
  `]
})
export class DashboardComponent {}
