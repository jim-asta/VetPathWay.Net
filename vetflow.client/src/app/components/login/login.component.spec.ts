import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LoginComponent } from './login.component';
import { testProviders } from '../../app.config';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent, FormsModule, ReactiveFormsModule],
      providers: [testProviders]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have a form with email and password controls', () => {
    expect(component.loginForm.contains('email')).toBeTrue();
    expect(component.loginForm.contains('password')).toBeTrue();
  });

  it('should make email and password controls required', () => {
    const emailControl = component.loginForm.get('email');
    const passwordControl = component.loginForm.get('password');
    emailControl?.setValue('');
    passwordControl?.setValue('');
    expect(emailControl?.valid).toBeFalse();
    expect(passwordControl?.valid).toBeFalse();
  });

  it('should emit login event when form is valid and submitted', () => {
    spyOn(component, 'onLoginLocal');
    component.loginForm.setValue({ email: 'test', password: 'pass', rememberMe: false });
    const form = fixture.nativeElement.querySelector('form');
    form.dispatchEvent(new Event('submit'));
    expect(component.onLoginLocal).toHaveBeenCalled();
  });
});
