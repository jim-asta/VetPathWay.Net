import { ApplicationConfig } from '@angular/core';
import { provideAnimations, provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import { definePreset } from '@primeng/themes';
import { withInterceptors } from '@angular/common/http';
import { authInterceptor } from '@core/interceptors/auth.interceptor';
import Aura from '@primeng/themes/aura';

import { routes } from './app.routes';
import { provideHttpClientTesting } from '@angular/common/http/testing';

export const baseProviders = [
  provideRouter(routes),
  providePrimeNG({
    theme: {
      preset: definePreset(Aura, {
        semantic: {
          primary: {
            50: '{amber.50}',
            100: '{amber.100}',
            200: '{amber.200}',
            300: '{amber.300}',
            400: '{amber.400}',
            500: '{amber.500}',
            600: '{amber.600}',
            700: '{amber.700}',
            800: '{amber.800}',
            900: '{amber.900}',
            950: '{amber.950}',
          },
          colorScheme: {
            light: {
              primary: {
                color: '{amber.500}',
                inverseColor: '#ffffff',
                hoverColor: '{amber.600}',
                activeColor: '{amber.500}',
              },
              highlight: {
                background: '{amber.650}',
                focusBackground: '{amber.500}',
                color: '#ffffff',
                focusColor: '#ffffff',
              },
            },
            dark: {
              primary: {
                color: '{amber.50}',
                inverseColor: '{amber.950}',
                hoverColor: '{amber.100}',
                activeColor: '{amber.200}',
              },
              highlight: {
                background: 'rgba(250, 250, 250, .16)',
                focusBackground: 'rgba(250, 250, 250, .24)',
                color: 'rgba(255,255,255,.87)',
                focusColor: 'rgba(255,255,255,.87)',
              },
            },
          },
        },
      })
    }
  })];

// Production
export const appConfig: ApplicationConfig = {
  providers: [
    ...baseProviders,
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimations(),
  ]
};
// Testing
export const testProviders = [
  ...baseProviders,
  provideHttpClient(withInterceptors([authInterceptor])),
  provideHttpClientTesting(),
  provideNoopAnimations(), // Faster than real animations
];
