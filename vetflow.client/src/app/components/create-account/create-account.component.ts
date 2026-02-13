import { Component, OnDestroy, OnInit } from '@angular/core';
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
import { AuthService } from '../../core/services/auth.service';

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
  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router, private messageService: MessageService) { }

  private subscription?: Subscription;
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
      passwordConfirm: ['', [passwordMatchValidator(() => this.password)]]
    });

    // Re-validate passwordConfirm when password changes so errors show on passwordConfirm
    this.password?.valueChanges.subscribe(() => {
      if (this.password?.value) {
        this.passwordConfirm?.markAsTouched();
      }
      this.passwordConfirm?.updateValueAndValidity();
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onSignup(): void {
    if (this.isLoading) return;   // Prevent double submit

    if (this.createAccountForm.valid) {
      this.isLoading = true;

      const { email, password, passwordConfirm } = this.createAccountForm.value;

      this.authService.signup(email, password, passwordConfirm).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Account creation successful! Please log in with the new user.',
            life: 3000
          });

          this.router.navigate(['/login']);
          this.isLoading = false;
        },
        error: err => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Invalid email or password. Please try again.',
            life: 3000
          });
          console.log(err.error?.message ?? err.error?.error ?? err.message ?? 'Unknown error occurred');
          this.isLoading = false;
        }
      });
    } else {
      this.markFormGroupTouched(this.createAccountForm);
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all required fields correctly.',
        life: 4000
      });
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
