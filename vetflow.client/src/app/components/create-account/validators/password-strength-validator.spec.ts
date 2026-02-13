import { FormControl } from '@angular/forms';
import { passwordStrengthValidator, categories } from './password-strength-validator';
import { TestBed } from '@angular/core/testing';
import { AppComponent } from '../../../app.component';
import { testProviders } from '../../../app.config';

describe('passwordStrengthValidator', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [testProviders],
    }).compileComponents();
  });

  describe('categories functions', () => {
    it('should detect uppercase letters', () => {
      expect(categories.uppercase('ABC')).toBe(true);
      expect(categories.uppercase('Abc')).toBe(true);
      expect(categories.uppercase('abc')).toBe(false);
      expect(categories.uppercase('123')).toBe(false);
    });

    it('should detect lowercase letters', () => {
      expect(categories.lowercase('abc')).toBe(true);
      expect(categories.lowercase('Abc')).toBe(true);
      expect(categories.lowercase('ABC')).toBe(false);
      expect(categories.lowercase('123')).toBe(false);
    });

    it('should detect numbers', () => {
      expect(categories.numbers('123')).toBe(true);
      expect(categories.numbers('abc123')).toBe(true);
      expect(categories.numbers('abc')).toBe(false);
    });

    it('should detect symbols', () => {
      expect(categories.symbols('!')).toBe(true);
      expect(categories.symbols('@#$')).toBe(true);
      expect(categories.symbols('abc123')).toBe(false);
      expect(categories.symbols('Test!123')).toBe(true);
    });
  });

  describe('validator function', () => {
    it('should return null for empty control value', () => {
      const control = new FormControl('');
      const validator = passwordStrengthValidator({ reqCategories: 3 });

      expect(validator(control)).toBeNull();
    });

    it('should return null for null control value', () => {
      const control = new FormControl(null);
      const validator = passwordStrengthValidator({ reqCategories: 3 });

      expect(validator(control)).toBeNull();
    });

    describe('with reqCategories requirement', () => {
      it('should return null when meeting minimum category requirements', () => {
        const control = new FormControl('Test123!');
        const validator = passwordStrengthValidator({ reqCategories: 3 });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should return null when exceeding minimum category requirements', () => {
        const control = new FormControl('Test123!');
        const validator = passwordStrengthValidator({ reqCategories: 2 });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should return minCategories error when not meeting requirements', () => {
        const control = new FormControl('test123'); // only lowercase and numbers
        const validator = passwordStrengthValidator({ reqCategories: 3 });

        const result = validator(control);

        expect(result).not.toBeNull();
        expect(result?.['minCategories']).toBeDefined();
        expect(result?.['minCategories'].required).toBe(3);
        expect(result?.['minCategories'].actual).toBe(2);
      });

      it('should include all category results in minCategories error', () => {
        const control = new FormControl('testonly');
        const validator = passwordStrengthValidator({ reqCategories: 3 });

        const result = validator(control);

        expect(result?.['minCategories'].categories).toEqual({
          uppercase: false,
          lowercase: true,
          numbers: false,
          symbols: false
        });
      });

      it('should pass with exactly the required number of categories', () => {
        const control = new FormControl('Test123'); // uppercase, lowercase, numbers (3 categories)
        const validator = passwordStrengthValidator({ reqCategories: 3 });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should fail with one less than required categories', () => {
        const control = new FormControl('test123'); // lowercase, numbers (2 categories)
        const validator = passwordStrengthValidator({ reqCategories: 3 });

        const result = validator(control);

        expect(result).not.toBeNull();
        expect(result?.['minCategories'].actual).toBe(2);
      });
    });

    describe('with individual category requirements', () => {
      it('should return null when all required categories are present', () => {
        const control = new FormControl('Test123!');
        const validator = passwordStrengthValidator({
          uppercase: true,
          lowercase: true,
          numbers: true,
          symbols: true
        });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should return uppercase error when missing uppercase', () => {
        const control = new FormControl('test123!');
        const validator = passwordStrengthValidator({ uppercase: true });

        const result = validator(control);

        expect(result).toEqual({ uppercase: false });
      });

      it('should return lowercase error when missing lowercase', () => {
        const control = new FormControl('TEST123!');
        const validator = passwordStrengthValidator({ lowercase: true });

        const result = validator(control);

        expect(result).toEqual({ lowercase: false });
      });

      it('should return numbers error when missing numbers', () => {
        const control = new FormControl('Test!');
        const validator = passwordStrengthValidator({ numbers: true });

        const result = validator(control);

        expect(result).toEqual({ numbers: false });
      });

      it('should return symbols error when missing symbols', () => {
        const control = new FormControl('Test123');
        const validator = passwordStrengthValidator({ symbols: true });

        const result = validator(control);

        expect(result).toEqual({ symbols: false });
      });

      it('should return multiple errors when multiple categories are missing', () => {
        const control = new FormControl('test');
        const validator = passwordStrengthValidator({
          uppercase: true,
          numbers: true,
          symbols: true
        });

        const result = validator(control);

        expect(result).toEqual({
          uppercase: false,
          numbers: false,
          symbols: false
        });
      });

      it('should not report errors for non-required categories', () => {
        const control = new FormControl('test'); // only lowercase
        const validator = passwordStrengthValidator({
          lowercase: true
        });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should only report errors for required categories that are missing', () => {
        const control = new FormControl('Test'); // has uppercase and lowercase
        const validator = passwordStrengthValidator({
          uppercase: true,
          lowercase: true,
          numbers: true
        });

        const result = validator(control);

        expect(result).toEqual({ numbers: false });
      });
    });

    describe('with mixed requirements', () => {
      it('should prioritize reqCategories over individual requirements', () => {
        const control = new FormControl('Test123'); // 3 categories: upper, lower, numbers
        const validator = passwordStrengthValidator({
          uppercase: true,
          lowercase: true,
          numbers: true,
          symbols: true, // This is missing
          reqCategories: 3 // But 3 categories are met
        });

        const result = validator(control);

        // Should pass because reqCategories is satisfied
        expect(result).toBeNull();
      });
    });

    describe('with no requirements', () => {
      it('should return null when no requirements are specified', () => {
        const control = new FormControl('anything');
        const validator = passwordStrengthValidator({});

        const result = validator(control);

        expect(result).toBeNull();
      });
    });

    describe('edge cases', () => {
      it('should handle password with all special characters', () => {
        const control = new FormControl('!@#$%^&*');
        const validator = passwordStrengthValidator({ symbols: true });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should handle password with spaces', () => {
        const control = new FormControl('Test 123 !');
        const validator = passwordStrengthValidator({ reqCategories: 4 });

        const result = validator(control);

        expect(result).toBeNull();
      });

      it('should handle very long passwords', () => {
        const control = new FormControl('Test123!'.repeat(50));
        const validator = passwordStrengthValidator({ reqCategories: 4 });

        const result = validator(control);

        expect(result).toBeNull();
      });
    });
  });
});
