import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Validator to check if two passwords match.
 * @param passwordControl - The control of the password field to compare.
 * @returns A ValidatorFn that checks if the passwords match.
 */
export function passwordMatchValidator(getPasswordControl: () => AbstractControl | null): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = getPasswordControl()?.value;
    const confirmPassword = control.value;

    if (password !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  };
}
