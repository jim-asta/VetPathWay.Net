import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TabsModule } from 'primeng/tabs';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { Title } from '@angular/platform-browser';

interface PricingPlan {
  name: string;
  price: string;
  patients: string;
  description: string;
}

@Component({
  selector: 'app-terms-of-service',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    TabsModule,
    ButtonModule,
    MessageModule,
    TagModule,
    DividerModule
  ],
  templateUrl: './terms-of-service.component.html',
  styleUrl: './terms-of-service.component.scss'
})
export class TermsOfServiceComponent implements OnInit {
  private titleService = inject(Title);

  effectiveDate = new Date();

  pricingPlans: PricingPlan[] = [
    {
      name: 'Starter',
      price: 'Free',
      patients: 'Up to 50 patients',
      description: 'Perfect for new practices. Includes basic scheduling, simple billing, community support, and full access to our open source code-base.'
    },
    {
      name: 'Professional',
      price: '$89/month',
      patients: 'Up to 1,000 patients',
      description: 'For growing practices. Advanced scheduling, inventory management, email support, and mobile app access with offline capabilities.'
    },
    {
      name: 'Practice',
      price: '$189/month',
      patients: 'Up to 5,000 patients',
      description: 'Complete practice solution. Multi-user support for up to 5 users, advanced analytics, payment processing, and priority support.'
    },
    {
      name: 'Enterprise',
      price: '$399/month',
      patients: 'Unlimited',
      description: 'Full-featured platform. Unlimited users, white-label options, custom integrations, dedicated support, and SLA guarantees.'
    }
  ];

  ngOnInit(): void {
    this.titleService.setTitle('Terms of Service');
  }

  print(): void {
    window.print();
  }

  acceptTerms(): void {
    console.log('Terms accepted');
  }
}
