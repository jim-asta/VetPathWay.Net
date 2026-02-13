import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ForgotPasswordComponent } from './forgot-password.component';
import { testProviders } from '../../app.config';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MessageService } from 'primeng/api';

describe('ForgotPasswordComponent', () => {
  let component: ForgotPasswordComponent;
  let fixture: ComponentFixture<ForgotPasswordComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ForgotPasswordComponent],
      providers: [
        ...testProviders,
        {
          provide: DynamicDialogRef,
          useValue: {
            close: jasmine.createSpy('close'),
            destroy: jasmine.createSpy('destroy')
          }
        },
        {
          provide: DynamicDialogConfig,
          useValue: {
            data: {}
          }
        },
        {
          provide: MessageService,
          useValue: {
            add: jasmine.createSpy('add'),
            clear: jasmine.createSpy('clear')
          }
        },
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ForgotPasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
