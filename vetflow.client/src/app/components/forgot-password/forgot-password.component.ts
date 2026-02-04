import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { AuthService } from '../../core/services/auth.service';
import { PasswordStrengthRequirements } from '../create-account/validators/password-strength-validator';

@Component({
  selector: 'app-forgot-password',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    PasswordModule,
    MessageModule
  ],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent implements OnInit {
  private requirements: PasswordStrengthRequirements = {
    uppercase: true,
    lowercase: true,
    numbers: true,
    symbols: true,
    reqCategories: 3
  }

  resetForm!: FormGroup;
  isLoading = false;

  constructor(private fb: FormBuilder, private dialogRef: DynamicDialogRef, private dialogConfig: DynamicDialogConfig, private authService: AuthService, private messageService: MessageService) { };

  ngOnInit(): void {
    this.resetForm = this.fb.nonNullable.group({    // Don't allow any values to be null
      email: [this.dialogConfig.data?.email || '', [Validators.required, Validators.email]]
    });
  }

  onResetPassword(): void {
    if (this.resetForm.valid) {
      this.isLoading = true;
      const { email } = this.resetForm.value;

      this.authService.forgotPassword(email).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Password Change Email Sent',
            detail: 'A password reset email has been sent.  Please use the link to reset your password and login.',
            life: 5000
          });
          this.dialogRef.close(true);
        },
        error: () => {
          // For security, show same message even on error
          this.messageService.add({
            severity: 'success',
            summary: 'Password Change Email Sent',
            detail: 'A password reset email has been sent.  Please use the link to reset your password and login.',
            life: 5000
          });
          this.dialogRef.close(true);
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Password ResetError',
        detail: 'The email entered was not a vaild email.',
        life: 5000
      });
      this.dialogRef.close(true);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  get email() {
    return this.resetForm.get('email');
  }
}
