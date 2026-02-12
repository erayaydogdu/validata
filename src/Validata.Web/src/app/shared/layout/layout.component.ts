import { Component, inject, signal, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { NAV_ITEMS, NavItem } from '../../core/config/navigation.config';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  styles: [],
  template: `
    <!-- Mobile backdrop -->
    @if (sidebarOpen()) {
      <div
        class="fixed inset-0 z-40 bg-gray-900/50 lg:hidden"
        (click)="sidebarOpen.set(false)">
      </div>
    }

    <!-- Sidebar -->
    <aside
      class="fixed inset-y-0 left-0 z-50 flex w-64 flex-col bg-gray-900 transition-transform duration-300 lg:translate-x-0"
      [class.-translate-x-full]="!sidebarOpen()"
      [class.translate-x-0]="sidebarOpen()">

      <!-- Brand -->
      <div class="flex h-16 items-center gap-2 px-6">
        <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-500">
          <svg class="h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75 11.25 15 15 9.75m-3-7.036A11.959 11.959 0 0 1 3.598 6 11.99 11.99 0 0 0 3 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285Z"/>
          </svg>
        </div>
        <span class="text-lg font-semibold text-white">Validata</span>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 space-y-1 px-3 py-4">
        @for (item of filteredNavItems(); track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="bg-gray-800 text-white"
            [routerLinkActiveOptions]="{ exact: item.route === '/dashboard' }"
            class="group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-300 transition-colors hover:bg-gray-800 hover:text-white"
            (click)="sidebarOpen.set(false)">
            <span class="h-5 w-5 shrink-0" [innerHTML]="item.icon"></span>
            {{ item.label }}
          </a>
        }
      </nav>
    </aside>

    <!-- Main area -->
    <div class="lg:pl-64">
      <!-- Header -->
      <header class="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-gray-200 bg-white px-4 sm:px-6 lg:px-8">
        <!-- Mobile hamburger -->
        <button
          class="lg:hidden -m-2.5 p-2.5 text-gray-700"
          (click)="sidebarOpen.set(true)">
          <svg class="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5"/>
          </svg>
        </button>

        <div class="hidden lg:block"></div>

        <!-- User menu -->
        <div class="relative">
          <button
            class="flex items-center gap-3 rounded-lg p-1.5 text-sm hover:bg-gray-50"
            (click)="toggleUserMenu($event)">
            <div class="flex h-8 w-8 items-center justify-center rounded-full bg-brand-500 text-xs font-semibold text-white">
              {{ getUserInitials() }}
            </div>
            <span class="hidden sm:block font-medium text-gray-700">{{ user()?.firstName }} {{ user()?.lastName }}</span>
            <svg class="hidden sm:block h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="m19.5 8.25-7.5 7.5-7.5-7.5"/>
            </svg>
          </button>

          @if (userMenuOpen()) {
            <div class="absolute right-0 top-full mt-2 w-64 rounded-xl border border-gray-200 bg-white py-2 shadow-lg">
              <div class="border-b border-gray-100 px-4 py-3">
                <p class="text-sm font-semibold text-gray-900">{{ user()?.firstName }} {{ user()?.lastName }}</p>
                <p class="text-xs text-gray-500">{{ user()?.email }}</p>
                <span class="mt-1.5 inline-block rounded-full bg-brand-50 px-2.5 py-0.5 text-xs font-medium text-brand-700">
                  {{ user()?.role }}
                </span>
              </div>
              <button
                class="flex w-full items-center gap-2 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50"
                (click)="signOut()">
                <svg class="h-4 w-4 text-gray-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 9V5.25A2.25 2.25 0 0 0 13.5 3h-6a2.25 2.25 0 0 0-2.25 2.25v13.5A2.25 2.25 0 0 0 7.5 21h6a2.25 2.25 0 0 0 2.25-2.25V15m3 0 3-3m0 0-3-3m3 3H9"/>
                </svg>
                Sign out
              </button>
            </div>
          }
        </div>
      </header>

      <!-- Page content -->
      <main class="p-4 sm:p-6 lg:p-8">
        <router-outlet />
      </main>
    </div>
  `
})
export class LayoutComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  user = toSignal(this.authService.currentUser$);
  sidebarOpen = signal(false);
  userMenuOpen = signal(false);

  filteredNavItems = signal<NavItem[]>([]);

  constructor() {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.filteredNavItems.set(
          NAV_ITEMS.filter(item => item.roles.includes(user.role))
        );
      } else {
        this.filteredNavItems.set([]);
      }
    });
  }

  getUserInitials(): string {
    const u = this.user();
    if (!u) return '?';
    return (u.firstName?.[0] || '') + (u.lastName?.[0] || '');
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.userMenuOpen.update(v => !v);
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.userMenuOpen.set(false);
  }

  signOut(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
