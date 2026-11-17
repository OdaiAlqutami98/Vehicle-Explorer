import { Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AutoComplete, AutoCompleteCompleteEvent } from 'primeng/autocomplete';
import { Button } from 'primeng/button';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { ProgressBar } from 'primeng/progressbar';
import { Select } from 'primeng/select';
import { LoadStatus } from '../../state/explorer-state';
import { Make } from '../../interfaces/make.interface';
import { VehicleType } from '../../interfaces/vehicle-type.interface';

const MAX_MAKE = 50;

@Component({
  selector: 'app-filter-card',
  imports: [FormsModule, AutoComplete, Button, IconField, InputIcon, ProgressBar, Select],
  templateUrl: './filter-card.html',
  styleUrl: './filter-card.scss',
})
export class FilterCard {
  readonly makes = input.required<Make[]>();
  readonly makesStatus = input.required<LoadStatus>();
  readonly vehicleTypes = input.required<VehicleType[]>();
  readonly vehicleTypesStatus = input.required<LoadStatus>();
  readonly years = input.required<number[]>();
  readonly selectedMake = input.required<Make | null>();
  readonly selectedYear = input.required<number | null>();
  readonly selectedVehicleType = input.required<string | null>();
  readonly makeChange = output<Make | null>();
  readonly yearChange = output<number | null>();
  readonly vehicleTypeChange = output<string | null>();
  readonly showModels = output<void>();
  protected readonly makeSuggestions = signal<Make[]>([]);
  protected readonly makeInputValue = signal<Make | string | null>(null);
  protected readonly canShowModels = computed(
    () => this.selectedMake() !== null && this.selectedYear() !== null,
  );

  protected readonly makePlaceholder = computed(() => {
    switch (this.makesStatus()) {
      case 'loading':
        return 'Loading makes…';
      case 'error':
        return 'Makes could not be loaded';
      default:
        return 'Start typing, e.g. Honda';
    }
  });

  protected readonly vehicleTypePlaceholder = computed(() => {
    switch (this.vehicleTypesStatus()) {
      case 'loading':
        return 'Loading types…';
      case 'error':
        return 'Types could not be loaded, all types are searched';
      default:
        return 'All types';
    }
  });

  protected searchMakes(event: AutoCompleteCompleteEvent): void {
    const query = event.query.trim().toLowerCase();
    const startsWith = this.makes().filter((make) => make.name.toLowerCase().startsWith(query));
    const contains = this.makes().filter(
      (make) =>
        !make.name.toLowerCase().startsWith(query) && make.name.toLowerCase().includes(query),
    );

    this.makeSuggestions.set([...startsWith, ...contains].slice(0, MAX_MAKE));
  }

  protected onMakeInputChange(value: Make | string | null): void {
    this.makeInputValue.set(value);
    const make = value !== null && typeof value === 'object' ? value : null;

    if (make?.id !== this.selectedMake()?.id) {
      this.makeChange.emit(make);
    }
  }
}
