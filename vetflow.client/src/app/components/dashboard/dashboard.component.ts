import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// PrimeNG Components
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { MenubarModule } from 'primeng/menubar';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { MenuItem } from 'primeng/api';

import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    MenubarModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  menuItems: MenuItem[] = [];

  constructor(
    private router: Router,
    private apiService: ApiService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.initializeMenu();
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

  logout(): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Logged Out',
      detail: 'You have been successfully logged out'
    });

    setTimeout(() => {
      this.router.navigate(['/login']);
    }, 1000);
  }

  testApiConnection(): void {
    this.apiService.testConnection().subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'API Connected',
          detail: 'Successfully connected to the API'
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'warn',
          summary: 'API Connection',
          detail: 'Could not connect to API - this is expected in skeleton mode'
        });
      }
    });
  }
}
