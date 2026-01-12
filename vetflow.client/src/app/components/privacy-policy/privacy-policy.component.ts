import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ChipModule } from 'primeng/chip';
import { TableModule } from 'primeng/table';
import { PanelModule } from 'primeng/panel';
import { DividerModule } from 'primeng/divider';
import { Title } from '@angular/platform-browser';
import { TitleStrategy } from '@angular/router';

interface ThirdParty {
  name: string;
  purpose: string;
}

@Component({
  selector: 'app-privacy-policy',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    AccordionModule,
    ButtonModule,
    MessageModule,
    ChipModule,
    TableModule,
    PanelModule,
    DividerModule
  ],
  templateUrl: './privacy-policy.component.html',
  styleUrl: './privacy-policy.component.scss'
})

export class PrivacyPolicyComponent implements OnInit {
  private titleService = inject(Title);
  lastUpdated = new Date();

  thirdParties: ThirdParty[] = [
    { name: 'Microsoft Azure', purpose: 'Cloud infrastructure and data storage' },
    { name: 'Payment Processors', purpose: 'Secure payment processing' },
    { name: 'Email Services', purpose: 'Transactional emails and notifications' },
    { name: 'Analytics Providers', purpose: 'Usage analytics and monitoring' }
  ];

  ngOnInit(): void {
    this.titleService.setTitle('Privacy Policy');
  }

  print(): void {
    window.print();
  }

  acceptPolicy(): void {
    console.log('Policy accepted');
  }
}
