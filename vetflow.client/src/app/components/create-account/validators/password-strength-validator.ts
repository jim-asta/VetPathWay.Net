import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const categories = {
  uppercase: (val: any) => /[A-Z]/.test(val),
  lowercase: (val: any) => /[a-z]/.test(val),
  numbers: (val: any) => /[0-9]/.test(val),
  symbols: (val: any) => /[@#$%^&*\-_!+=[\]{}|\\:',.\?\/`~"();<> ]/.test(val)
} as const;

// Derive and export the type - fields come from categories
export type PasswordStrengthRequirements = {
  [K in keyof typeof categories]?: boolean;
} & {
  reqCategories?: number; // Minimum number of categories required
};

export function passwordStrengthValidator(
  requirements: PasswordStrengthRequirements = {}
): ValidatorFn {

  return (control: AbstractControl): ValidationErrors | null => {
    let errors: ValidationErrors = {};

    if (!control?.value) return null;

    const results = Object.fromEntries(   // Run the tests on each requirement
      Object.entries(categories).map(([key, fn]) => [key, fn(control.value)])
    );
                                         // Test each category on a control value
    if (requirements.reqCategories) {    // Count the categories that are met (=== true) if

      if (Object.values(results).filter(c => c === true).length >= requirements.reqCategories)   // If there are more matches than we need, exit
        return null;
    
      // if we have a minCategories
      errors['minCategories'] = {
        required: requirements.reqCategories,
        actual: Object.values(results).filter(c => c === true).length,
        categories: { ...results } // Show ALL for UI display
      }
    } else {  // Report individual errors, no minCategories

      errors = {
        ...errors,  // Merge these errors into existing
        ...Object.fromEntries(
          Object.entries(results).filter(
            ([key, passed]) => requirements[key as keyof typeof requirements] && !passed        // Only include the category/error that is required and failed (we're reporting errors)
          )
        )
       }
      }

    return Object.keys(errors).length > 0 ? errors : null;
  };
}
