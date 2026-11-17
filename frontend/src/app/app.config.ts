import { provideHttpClient, withFetch } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { providePrimeNG } from 'primeng/config';
import { VehicleExplorerPreset } from './theme/vehicle-explorer-preset';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withFetch()),
    providePrimeNG({
      theme: {
        preset: VehicleExplorerPreset,
        options: { darkModeSelector: 'none' },
      },
      overlayAppendTo: 'body',
    }),
  ],
};
