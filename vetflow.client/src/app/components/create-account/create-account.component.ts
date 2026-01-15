import { Component, OnDestroy, OnInit, ViewEncapsulation, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { passwordMatchValidator } from './validators/password-match-validator';

// PrimeNG Imports
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { PasswordStrengthComponent } from './password-strength/password-strength.component';
import { PasswordStrengthRequirements, passwordStrengthValidator } from './validators/password-strength-validator';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-create-account',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
    CheckboxModule,
    DividerModule,
    MessageModule,
    ToastModule,
    PasswordStrengthComponent
  ],
  templateUrl: './create-account.component.html',
  styleUrl: './create-account.component.scss',
  providers: [MessageService]
})
export class CreateAccountComponent implements OnInit, OnDestroy {
  private subscription?: Subscription;
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private messageService = inject(MessageService);
  private requirements: PasswordStrengthRequirements = {
    uppercase: true,
    lowercase: true,
    numbers: true,
    symbols: true,
    reqCategories: 3
  }

  createAccountForm!: FormGroup;
  isLoading: boolean = false;

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.createAccountForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(256), passwordStrengthValidator(this.requirements)]],
      passwordConfirm: ['', [Validators.required, passwordMatchValidator(() => this.password)]]
    });

    // Re-validate passwordConfirm whenever password changes
    this.password?.valueChanges.subscribe(() => {
      this.passwordConfirm?.updateValueAndValidity();
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onSubmit(): void {
    if (this.createAccountForm.valid) {
      this.isLoading = true;

      // Simulate API call
      setTimeout(() => {
        const { email, password } = this.createAccountForm.value;

        // Mock authentication logic
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Account creation successful! Redirecting...',
          life: 3000
        });

        // Redirect to login after short delay
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 1500);

        this.isLoading = false;
      }, 1500);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  // Getters for form controls
  get email() {
    return this.createAccountForm?.get('email');
  }

  get password() {
    return this.createAccountForm?.get('password');
  }

  get passwordConfirm() {
    return this.createAccountForm?.get('passwordConfirm');
  }

  get rememberMe() {
    return this.createAccountForm?.get('rememberMe');
  }
}
