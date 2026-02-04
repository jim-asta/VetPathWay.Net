import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { ToastModule } from 'primeng/toast';
import { AvatarModule } from 'primeng/avatar';
import { BadgeModule } from 'primeng/badge';
import { ChipModule } from 'primeng/chip';
import { SkeletonModule } from 'primeng/skeleton';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { MessageService, MenuItem } from 'primeng/api';

import { AuthService } from '../../core/services/auth.service';
import { UserProfileService } from '@core/services/user-profile.service';
import { UserProfile } from '@DTOs/user-profile.dto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    MenubarModule,
    ToastModule,
    AvatarModule,
    BadgeModule,
    ChipModule,
    SkeletonModule,
    TagModule,
    DividerModule
  ],
  providers: [MessageService],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  constructor(private router: Router, private authService: AuthService, private userProfileService: UserProfileService, private messageService: MessageService) { }

  menuItems: MenuItem[] = [];
  userProfile: UserProfile | null = null;
  isLoadingProfile = true;
  profileError = false;

  ngOnInit(): void {
    this.initializeMenu();
    this.loadUserProfile();
  }

  private initializeMenu(): void {
    this.menuItems = [
      {
        label: 'Dashboard',
        icon: 'pi pi-home',
        routerLink: '/dashboard'
      },
      {
        label: 'Users',
        icon: 'pi pi-users',
        items: [
          { label: 'View All', icon: 'pi pi-list' },
          { label: 'Add New', icon: 'pi pi-plus' }
        ]
      },
      {
        label: 'Reports',
        icon: 'pi pi-chart-bar',
        items: [
          { label: 'Sales', icon: 'pi pi-dollar' },
          { label: 'Analytics', icon: 'pi pi-chart-line' }
        ]
      },
      {
        label: 'Settings',
        icon: 'pi pi-cog'
      }
    ];
  }

  private loadUserProfile(): void {
    this.isLoadingProfile = true;
    this.profileError = false;

    this.userProfileService.getUserProfile().subscribe({
      next: (profile) => {
        this.userProfile = profile;
        this.isLoadingProfile = false;

        this.messageService.add({
          severity: 'success',
          summary: 'Welcome!',
          detail: 'Successfully authenticated as ' + profile.name,
          life: 3000
        });
      },
      error: (err) => {
        this.isLoadingProfile = false;
        this.profileError = true;

        this.messageService.add({
          severity: 'error',
          summary: 'Authentication Failed',
          detail: 'Could not load user profile. Please log in again.',
          life: 5000
        });

        console.error('Profile load error:', err);

        // Redirect to login after delay
        setTimeout(() => {
          this.authService.logout();
        }, 3000);
      }
    });
  }

  logout(): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Logged Out',
      detail: 'You have been successfully logged out'
    });

    setTimeout(() => {
      this.authService.logout();
    }, 1000);
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
