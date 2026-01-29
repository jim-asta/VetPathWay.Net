import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { ForgotPasswordComponent } from '@components/forgot-password/forgot-password.component';

// PrimeNG Imports
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DividerModule } from 'primeng/divider';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { AuthService } from '@services/auth.service';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-login',
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
    DialogModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  providers: [MessageService, DialogService]
})
export class LoginComponent implements OnInit, OnDestroy {
  constructor(private fb: FormBuilder, private router: Router, private messageService: MessageService, private authService: AuthService, private dialogService: DialogService) { }

  loginForm!: FormGroup;
  isLoading: boolean = false;
  dialogRef: DynamicDialogRef | undefined;

  ngOnInit(): void {
    this.initializeForm();
    this.email?.setValue(localStorage.getItem('rememberMeEmail'));
  }

  private initializeForm(): void {
    this.loginForm = this.fb.nonNullable.group({    // Don't allow any values to be null
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngAfterViewInit(): void {
    if (('credentials' in navigator)) {

      const opts = { password: true, mediation: 'optional' } as any;

      navigator.credentials.get(opts)
        .then((cred: any) => {
          if (cred && cred.id && cred.password) {
            console.log('Autofill from Chrome credentials API:', cred);
            this.loginForm.patchValue({
              email: cred.id,
              password: cred.password
            });
            this.loginForm.markAllAsTouched();
            this.loginForm.updateValueAndValidity();
          }
        })
        .catch(err => console.debug('No credentials returned:', err));
    }
  }

  ngOnDestroy(): void {
    if (this.dialogRef) {
      this.dialogRef.close();
    }
  }

  onLoginLocal(): void {
     if (this.loginForm.valid) {
      this.isLoading = true;

       const { email, password, rememberMe } = this.loginForm.value;

       this.authService.loginLocal(email, password).subscribe({
         next: () => {
           this.messageService.add({
             severity: 'success',
             summary: 'Success',
             detail: 'Login successful! Redirecting...',
             life: 3000
           });

           // Handle remember me if this is a vaild login
           if (rememberMe) {
             localStorage.setItem('rememberMe', 'true');
             localStorage.setItem('rememberMeEmail', email);
           }

           this.router.navigate(['/dashboard']);
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
      this.markFormGroupTouched(this.loginForm);
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all required fields correctly.',
        life: 4000
      });
    }
  }

  loginGoogle() {
    this.authService.loginSso('google');
  }

  loginMicrosoft() {
    this.authService.loginSso('microsoft');
  }

  onForgotPassword(event: Event): void {
    event.preventDefault();   // Don't navigate to "#"

    this.dialogRef = this.dialogService.open(ForgotPasswordComponent, {
      modal: true,
      dismissableMask: true,
      showHeader: false,  // Hide default header
      contentStyle: {
        overflow: 'hidden',  // Prevent scrollbars
        borderRadius: '0.75rem',  // Match login card
      },
      styleClass: 'w-[26.25rem] !border-0 !m-4',  // Match login card
      data: {
        email: this.email?.valid ? this.email.value : ''
      }
    });
  }

  socialLogin(provider: string): void {
    this.messageService.add({
      severity: 'info',
      summary: 'Social Login',
      detail: `Redirecting to ${provider} login...`,
      life: 3000
    });

    // Implement social login logic here
    console.log(`Attempting ${provider} login`);
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
  get email(): AbstractControl<any, any> | null {
    return this.loginForm?.get('email');
  }

  get password(): AbstractControl<any, any> | null {
    return this.loginForm?.get('password');
  }

  get rememberMe(): AbstractControl<any, any> | null {
    return this.loginForm?.get('rememberMe');
  }
}
