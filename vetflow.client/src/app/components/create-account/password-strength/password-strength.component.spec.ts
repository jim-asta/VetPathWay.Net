import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl } from '@angular/forms';
import { PasswordStrengthComponent } from './password-strength.component';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { KeyValuePipe, NgClass } from '@angular/common';
import { testProviders } from '../../../app.config';

describe('PasswordStrengthComponent', () => {
  let component: PasswordStrengthComponent;
  let fixture: ComponentFixture<PasswordStrengthComponent>;

  beforeAll(async () => {
    await TestBed.configureTestingModule({
      imports: [
        PasswordStrengthComponent,
        ProgressBarModule,
        TagModule,
        DividerModule,
        NgClass,
        KeyValuePipe
      ],
      providers: [testProviders]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PasswordStrengthComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('results getter', () => {
    it('should return empty object when control is null', () => {
      component.control = null;

      expect(component.results).toEqual({});
    });

    it('should return empty object when control value is empty', () => {
      component.control = new FormControl('');

      expect(component.results).toEqual({});
    });

    it('should return empty object when control value is null', () => {
      component.control = new FormControl(null);

      expect(component.results).toEqual({});
    });

    it('should return all false when password has no special characteristics', () => {
      component.control = new FormControl('password');

      const results = component.results;

      expect(results['uppercase']).toBe(false);
      expect(results['lowercase']).toBe(true);
      expect(results['numbers']).toBe(false);
      expect(results['symbols']).toBe(false);
    });

    it('should detect uppercase letters', () => {
      component.control = new FormControl('Password');

      const results = component.results;

      expect(results['uppercase']).toBe(true);
      expect(results['lowercase']).toBe(true);
    });

    it('should detect numbers', () => {
      component.control = new FormControl('password123');

      const results = component.results;

      expect(results['numbers']).toBe(true);
    });

    it('should detect symbols', () => {
      component.control = new FormControl('password!');

      const results = component.results;

      expect(results['symbols']).toBe(true);
    });

    it('should detect all categories in strong password', () => {
      component.control = new FormControl('Test123!');

      const results = component.results;

      expect(results['uppercase']).toBe(true);
      expect(results['lowercase']).toBe(true);
      expect(results['numbers']).toBe(true);
      expect(results['symbols']).toBe(true);
    });
  });

  describe('score getter', () => {
    it('should return 0 when control is null', () => {
      component.control = null;

      expect(component.score).toBe(0);
    });

    it('should return 0 when password is empty', () => {
      component.control = new FormControl('');

      expect(component.score).toBe(0);
    });

    it('should return 1 for password with only lowercase', () => {
      component.control = new FormControl('password');

      expect(component.score).toBe(1);
    });

    it('should return 2 for password with lowercase and uppercase', () => {
      component.control = new FormControl('Password');

      expect(component.score).toBe(2);
    });

    it('should return 3 for password with lowercase, uppercase, and numbers', () => {
      component.control = new FormControl('Password123');

      expect(component.score).toBe(3);
    });

    it('should return 4 for password with all categories', () => {
      component.control = new FormControl('Test123!');

      expect(component.score).toBe(4);
    });
  });

  describe('strength getter', () => {
    beforeEach(() => {
      component.requirements = { reqCategories: 4 };
    });

    it('should return "None" when score is 0', () => {
      component.control = new FormControl('');

      expect(component.strength).toBe('None');
    });

    it('should return "Weak" when score is less than half of minimum', () => {
      component.control = new FormControl('password');
      component.requirements = { reqCategories: 4 };

      expect(component.score).toBe(1);
      expect(component.strength).toBe('Weak');
    });

    it('should return "Good" when score is between half and minimum', () => {
      component.control = new FormControl('Password123');
      component.requirements = { reqCategories: 4 };

      expect(component.score).toBe(3);
      expect(component.strength).toBe('Good');
    });

    it('should return "Strong" when score meets minimum', () => {
      component.control = new FormControl('Test123!');
      component.requirements = { reqCategories: 4 };

      expect(component.score).toBe(4);
      expect(component.strength).toBe('Strong');
    });

    it('should use default requirement of 4 when reqCategories is not set', () => {
      component.control = new FormControl('Test123!');
      component.requirements = {};

      expect(component.strength).toBe('Strong');
    });

    it('should calculate strength based on custom reqCategories', () => {
      component.control = new FormControl('Password123');
      component.requirements = { reqCategories: 3 };

      expect(component.score).toBe(3);
      expect(component.strength).toBe('Strong');
    });

    it('should return "Weak" for score 1 with reqCategories 3', () => {
      component.control = new FormControl('password');
      component.requirements = { reqCategories: 3 };

      expect(component.score).toBe(1);
      expect(component.strength).toBe('Weak');
    });

    it('should return "Good" for score 2 with reqCategories 3', () => {
      component.control = new FormControl('Password');
      component.requirements = { reqCategories: 3 };

      expect(component.score).toBe(2);
      expect(component.strength).toBe('Good');
    });
  });

  describe('barColorClass getter', () => {
    it('should return "progress-gray" for None strength', () => {
      component.control = new FormControl('');

      expect(component.barColorClass).toBe('progress-gray');
    });

    it('should return "progress-orange" for Weak strength', () => {
      component.control = new FormControl('password');
      component.requirements = { reqCategories: 4 };

      expect(component.barColorClass).toBe('progress-orange');
    });

    it('should return "progress-amber" for Good strength', () => {
      component.control = new FormControl('Password123');
      component.requirements = { reqCategories: 4 };

      expect(component.barColorClass).toBe('progress-amber');
    });

    it('should return "progress-green" for Strong strength', () => {
      component.control = new FormControl('Test123!');
      component.requirements = { reqCategories: 4 };

      expect(component.barColorClass).toBe('progress-green');
    });
  });

  describe('component rendering', () => {
    it('should display progress bar with correct value', () => {
      component.control = new FormControl('Test123!');
      component.requirements = { reqCategories: 4 };
      fixture.detectChanges();

      const progressBar = fixture.nativeElement.querySelector('p-progressbar');
      expect(progressBar).toBeTruthy();
    });

    it('should display strength tag', () => {
      component.control = new FormControl('Test123!');
      fixture.detectChanges();

      const tag = fixture.nativeElement.querySelector('p-tag');
      expect(tag).toBeTruthy();
    });

    it('should display category checks', () => {
      component.control = new FormControl('Test123!');
      fixture.detectChanges();

      const checks = fixture.nativeElement.querySelectorAll('.pi-check-circle');
      expect(checks.length).toBeGreaterThan(0);
    });

    it('should show check icons for met requirements', () => {
      component.control = new FormControl('Test123!');
      fixture.detectChanges();

      const checkIcons = fixture.nativeElement.querySelectorAll('.pi-check-circle');
      expect(checkIcons.length).toBe(4);
    });

    it('should show empty circle icons for unmet requirements', () => {
      component.control = new FormControl('test');
      fixture.detectChanges();

      const emptyIcons = fixture.nativeElement.querySelectorAll('.pi-circle-off');
      expect(emptyIcons.length).toBe(3);
    });
  });

  describe('edge cases', () => {
    it('should handle control without value property', () => {
      component.control = new FormControl(undefined);

      expect(component.results).toEqual({});
      expect(component.score).toBe(0);
      expect(component.strength).toBe('None');
    });

    it('should handle very long passwords', () => {
      component.control = new FormControl('Test123!'.repeat(50));

      expect(component.score).toBe(4);
      expect(component.strength).toBe('Strong');
    });

    it('should handle passwords with only symbols', () => {
      component.control = new FormControl('!@#$%^&*');

      expect(component.score).toBe(1);
      expect(component.results['symbols']).toBe(true);
    });

    it('should handle passwords with spaces', () => {
      component.control = new FormControl('Test 123 !');

      expect(component.score).toBe(4);
    });
  });
});
