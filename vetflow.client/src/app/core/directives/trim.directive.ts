import { Directive, HostListener, Optional, Self } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[trim]',
  standalone: true
})
export class TrimDirective {
  constructor(@Optional() @Self() private ngControl: NgControl) { }

  ngOnInit(): void {
    if (!this.ngControl?.control) return;

    const control = this.ngControl.control;

    control.valueChanges.subscribe(value => {   // Subscribe to valueChanges to trim automatically
      if (value && typeof value === 'string') {
        const trimmed = value.trim();

        if (trimmed !== value) {        // Only update if value changed to avoid infinite loops
          control.setValue(trimmed, { emitEvent: false });
        }
      }
    });
  }
}
