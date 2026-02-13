import { FormControl } from '@angular/forms';
import { passwordMatchValidator } from './password-match-validator';
import { TestBed } from '@angular/core/testing';
import { AppComponent } from '../../../app.component';
import { testProviders } from '../../../app.config';

describe('passwordMatchValidator', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [testProviders],
    }).compileComponents();
  });

  it('should return null when passwords match', () => {
    const passwordControl = new FormControl('Test123!');
    const confirmControl = new FormControl('Test123!');

    const validator = passwordMatchValidator(() => passwordControl);
    const result = validator(confirmControl);

    expect(result).toBeNull();
  });

  it('should return passwordMismatch error when passwords do not match', () => {
    const passwordControl = new FormControl('Test123!');
    const confirmControl = new FormControl('Test456!');

    const validator = passwordMatchValidator(() => passwordControl);
    const result = validator(confirmControl);

    expect(result).toEqual({ passwordMismatch: true });
  });

  it('should return passwordMismatch error when password is empty and confirm is not', () => {
    const passwordControl = new FormControl('');
    const confirmControl = new FormControl('Test123!');

    const validator = passwordMatchValidator(() => passwordControl);
    const result = validator(confirmControl);

    expect(result).toEqual({ passwordMismatch: true });
  });

  it('should return null when both passwords are empty', () => {
    const passwordControl = new FormControl('');
    const confirmControl = new FormControl('');

    const validator = passwordMatchValidator(() => passwordControl);
    const result = validator(confirmControl);

    expect(result).toBeNull();
  });

  it('should return passwordMismatch error when password control is null', () => {
    const confirmControl = new FormControl('Test123!');

    const validator = passwordMatchValidator(() => null);
    const result = validator(confirmControl);

    expect(result).toEqual({ passwordMismatch: true });
  });

  it('should handle case-sensitive password comparison', () => {
    const passwordControl = new FormControl('Test123!');
    const confirmControl = new FormControl('test123!');

    const validator = passwordMatchValidator(() => passwordControl);
    const result = validator(confirmControl);

    expect(result).toEqual({ passwordMismatch: true });
  });

  it('should handle whitespace differences', () => {
    const passwordControl = new FormControl('Test123!');
    const confirmControl = new FormControl('Test123! ');

    const validator = passwordMatchValidator(() => passwordControl);
    const result = validator(confirmControl);

    expect(result).toEqual({ passwordMismatch: true });
  });
});


















