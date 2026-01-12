import { Component, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { ProgressBarModule } from 'primeng/progressbar';
import { categories, PasswordStrengthRequirements } from '../validators/password-strength-validator';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { KeyValuePipe, NgClass } from '@angular/common';

@Component({
  selector: 'app-password-strength',
  standalone: true,
  imports: [
    ProgressBarModule,
    TagModule,
    DividerModule,
    NgClass,
    KeyValuePipe
  ],
  templateUrl: './password-strength.component.html',
  styleUrl: './password-strength.component.scss'
})
export class PasswordStrengthComponent {
  @Input() control!: AbstractControl | null;
  @Input() requirements: PasswordStrengthRequirements = {};

  get results(): Record<string, boolean> {
    if (!this.control?.value) return {};

    const value = this.control.value;

    return Object.fromEntries(    // Dynamically create the set of tests on value
            Object.entries(categories).map(([key, fn]) => [key, fn(value)])
    ) as Record<keyof typeof categories, boolean>;
  }

  get score(): number {
    return Object.values(this.results).filter(Boolean).length;
  }

  get strength(): string {
    const min = this.requirements.reqCategories ?? 4;
    if (this.score === 0) return 'None';
    if (this.score < min / 2) return 'Weak';
    if (this.score < min) return 'Good';
    return 'Strong';
  }

  get barColorClass(): string {
    switch (this.strength) {
      case 'Strong': return 'progress-green';
      case 'Good': return 'progress-amber';
      case 'Weak': return 'progress-orange';
      default: return 'progress-gray';
    }
  }
}
