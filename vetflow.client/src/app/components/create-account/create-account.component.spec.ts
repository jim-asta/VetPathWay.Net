import { ComponentFixture, fakeAsync, flush, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { of, Subject, throwError } from 'rxjs';
import { CreateAccountComponent } from './create-account.component';
import { AuthService } from '../../core/services/auth.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('CreateAccountComponent', () => {
  let component: CreateAccountComponent;
  let fixture: ComponentFixture<CreateAccountComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['signup']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [
        CreateAccountComponent,
        ReactiveFormsModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy },
        MessageService    // Have to use real one because Standalone component uses its own (i.e. providers: [MessageService])
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Initialization', () => {
    it('should initialize form with empty values', () => {
      expect(component.createAccountForm).toBeDefined();
      expect(component.email?.value).toBe('');
      expect(component.password?.value).toBe('');
      expect(component.passwordConfirm?.value).toBe('');
    });

    it('should have email control with required and email validators', () => {
      const emailControl = component.email;

      emailControl?.setValue('');
      expect(emailControl?.hasError('required')).toBe(true);

      emailControl?.setValue('invalid-email');
      expect(emailControl?.hasError('email')).toBe(true);

      emailControl?.setValue('valid@email.com');
      expect(emailControl?.valid).toBe(true);
    });

    it('should have password control with required, minLength, maxLength, and strength validators', () => {
      const passwordControl = component.password;

      passwordControl?.setValue('');
      expect(passwordControl?.hasError('required')).toBe(true);

      passwordControl?.setValue('short');
      expect(passwordControl?.hasError('minlength')).toBe(true);

      passwordControl?.setValue('a'.repeat(257));
      expect(passwordControl?.hasError('maxlength')).toBe(true);
    });

    it('should have passwordConfirm control with passwordMatch validator', () => {
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Different123!');
      component.passwordConfirm?.markAsTouched();

      expect(component.passwordConfirm?.hasError('passwordMismatch')).toBe(true);
    });

    it('should re-validate passwordConfirm when password changes', () => {
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');
      component.passwordConfirm?.updateValueAndValidity();

      expect(component.passwordConfirm?.valid).toBe(true);

      component.password?.setValue('Different123!');

      expect(component.passwordConfirm?.hasError('passwordMismatch')).toBe(true);
    });

    it('should mark passwordConfirm as touched when password has value and changes', () => {
      component.password?.setValue('Test123!');

      expect(component.passwordConfirm?.touched).toBe(true);
    });
  });

  describe('Form Validation', () => {
    it('should be invalid when all fields are empty', () => {
      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when email is missing', () => {
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when password is missing', () => {
      component.email?.setValue('test@example.com');
      component.passwordConfirm?.setValue('Test123!');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when passwordConfirm is missing', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test123!');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when email format is incorrect', () => {
      component.email?.setValue('invalid-email');
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when password is too short', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test1!');
      component.passwordConfirm?.setValue('Test1!');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when password is too long', () => {
      const longPassword = 'T'.repeat(257) + '123!';
      component.email?.setValue('test@example.com');
      component.password?.setValue(longPassword);
      component.passwordConfirm?.setValue(longPassword);

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when password does not meet strength requirements', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('testpassword'); // No uppercase, numbers, or symbols
      component.passwordConfirm?.setValue('testpassword');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be invalid when passwords do not match', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Different123!');

      expect(component.createAccountForm.valid).toBe(false);
    });

    it('should be valid when all fields are correct', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');

      expect(component.createAccountForm.valid).toBe(true);
    });

    it('should be valid with minimum strength password (3 categories)', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test1234'); // uppercase, lowercase, numbers
      component.passwordConfirm?.setValue('Test1234');

      expect(component.createAccountForm.valid).toBe(true);
    });
  });

  describe('onSignup()', () => {
    beforeEach(() => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');
    });

    it('should call authService.signup with correct parameters when form is valid', () => {
      authService.signup.and.returnValue(of(void 0));

      component.onSignup();

      expect(authService.signup).toHaveBeenCalledWith('test@example.com', 'Test123!', 'Test123!');
    });

    it('should set isLoading to true then false during signup', () => {
      const subject = new Subject<void>();
      authService.signup.and.returnValue(subject.asObservable());

      component.onSignup();

      expect(component.isLoading).toBe(true);

      subject.next();
      subject.complete();

      expect(component.isLoading).toBe(false);
    });

    it('should navigate to login page on successful signup', () => {
      authService.signup.and.returnValue(of(void 0));

      component.onSignup();

      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should show success message on successful signup', () => {
      authService.signup.and.returnValue(of(void 0));

      spyOn(component['messageService'], 'add');
      component.onSignup();

      expect(component['messageService'].add).toHaveBeenCalledWith({
        severity: 'success',
        summary: 'Success',
        detail: 'Account creation successful! Please log in with the new user.',
        life: 3000
      });
    });

    it('should set isLoading to false after successful signup', () => {
      authService.signup.and.returnValue(of(void 0));

      component.onSignup();

      expect(component.isLoading).toBe(false);
    });

    it('should show error message on failed signup', () => {
      const error = { error: { message: 'Email already exists' } };
      authService.signup.and.returnValue(throwError(() => error));

      spyOn(component['messageService'], 'add');
      component.onSignup();

      expect(component['messageService'].add).toHaveBeenCalledWith({
        severity: 'error',
        summary: 'Error',
        detail: 'Invalid email or password. Please try again.',
        life: 3000
      });
    });

    it('should set isLoading to false after failed signup', () => {
      const error = { error: { message: 'Email already exists' } };
      authService.signup.and.returnValue(throwError(() => error));

      component.onSignup();

      expect(component.isLoading).toBe(false);
    });

    it('should not call authService when form is invalid', () => {
      component.email?.setValue('');

      component.onSignup();

      expect(authService.signup).not.toHaveBeenCalled();
    });

    it('should mark all controls as touched when form is invalid', () => {
      component.email?.setValue('');

      component.onSignup();

      expect(component.email?.touched).toBe(true);
      expect(component.password?.touched).toBe(true);
      expect(component.passwordConfirm?.touched).toBe(true);
    });

    it('should show warning message when form is invalid', () => {
      component.email?.setValue('');

      spyOn(component['messageService'], 'add');
      component.onSignup();

      expect(component['messageService'].add).toHaveBeenCalledWith({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all required fields correctly.',
        life: 4000
      });
    });

    it('should handle error with error.error property', () => {
      const error = { error: { error: 'Database error' } };
      authService.signup.and.returnValue(throwError(() => error));
      spyOn(console, 'log');

      component.onSignup();

      expect(console.log).toHaveBeenCalledWith('Database error');
    });

    it('should handle error with message property', () => {
      const error = { message: 'Network error' };
      authService.signup.and.returnValue(throwError(() => error));
      spyOn(console, 'log');

      component.onSignup();

      expect(console.log).toHaveBeenCalledWith('Network error');
    });

    it('should handle error with no specific message', () => {
      const error = {};
      authService.signup.and.returnValue(throwError(() => error));
      spyOn(console, 'log');

      component.onSignup();

      expect(console.log).toHaveBeenCalledWith('Unknown error occurred');
    });

    it('should not navigate to login on failed signup', () => {
      const error = { error: { message: 'Error' } };
      authService.signup.and.returnValue(throwError(() => error));

      component.onSignup();

      expect(router.navigate).not.toHaveBeenCalled();
    });
  });

  describe('Form Control Getters', () => {
    it('should return email control', () => {
      expect(component.email).toBe(component.createAccountForm.get('email'));
    });

    it('should return password control', () => {
      expect(component.password).toBe(component.createAccountForm.get('password'));
    });

    it('should return passwordConfirm control', () => {
      expect(component.passwordConfirm).toBe(component.createAccountForm.get('passwordConfirm'));
    });
  });

  describe('ngOnDestroy()', () => {
    it('should unsubscribe from subscription', () => {
      const subscription = jasmine.createSpyObj('Subscription', ['unsubscribe']);
      (component as any).subscription = subscription;

      component.ngOnDestroy();

      expect(subscription.unsubscribe).toHaveBeenCalled();
    });

    it('should handle undefined subscription', () => {
      (component as any).subscription = undefined;

      expect(() => component.ngOnDestroy()).not.toThrow();
    });
  });

  describe('Edge Cases', () => {
    it('should not submit again while loading', () => {
      const subject = new Subject<void>();
      authService.signup.and.returnValue(subject.asObservable());

      component.email?.setValue('test@example.com');
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');

      component.onSignup();  // starts loading
      component.onSignup();  // should be ignored

      expect(authService.signup).toHaveBeenCalledTimes(1);

      subject.next();
      subject.complete();
    });

    it('should handle special characters in email', () => {
      component.email?.setValue('test+special@example.com');
      component.password?.setValue('Test123!');
      component.passwordConfirm?.setValue('Test123!');

      expect(component.createAccountForm.valid).toBe(true);
    });

    it('should handle password with all special character types', () => {
      component.email?.setValue('test@example.com');
      component.password?.setValue('Test123!@#$%^&*');
      component.passwordConfirm?.setValue('Test123!@#$%^&*');

      expect(component.createAccountForm.valid).toBe(true);
    });

    it('should trim whitespace not affect email validation', () => {
      const input = fixture.nativeElement.querySelector('#email') as HTMLInputElement;

      input.value = '  test@example.com  ';
      input.dispatchEvent(new Event('input'));    // Simulate actual input behavior because type='email' and PrimeNG pInputText trims but only if actual event

      fixture.detectChanges();

      // Email validator accepts spaces (though back-end should trim)
      expect(component.email?.hasError('email')).toBe(false);
    });
  });

  describe('Component Lifecycle', () => {
    it('should set isLoading to false initially', () => {
      expect(component.isLoading).toBe(false);
    });
  });
});
