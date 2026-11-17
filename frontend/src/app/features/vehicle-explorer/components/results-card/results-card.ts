import { Component, computed, input, linkedSignal, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Button } from 'primeng/button';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { ModelSearch } from '../../interfaces/model-search.interface';
import { VehicleModel } from '../../interfaces/vehicle-model.interface';

@Component({
  selector: 'app-results-card',
  imports: [FormsModule, Button, IconField, InputIcon, InputText, Message, TableModule, Tag],
  templateUrl: './results-card.html',
  styleUrl: './results-card.scss',
})
export class ResultsCard {
  readonly search = input.required<ModelSearch>();
  readonly models = input.required<VehicleModel[]>();
  readonly clearVehicleType = output<void>();

  protected readonly nameFilter = linkedSignal({
    source: this.models,
    computation: () => '',
  });

  protected readonly visibleModels = computed(() => {
    const filter = this.nameFilter().trim().toLowerCase();

    if (filter === '') {
      return this.models();
    }

    return this.models().filter((model) => model.name.toLowerCase().includes(filter));
  });

  protected readonly showVehicleType = computed(() => this.search().vehicleType !== null);

  protected readonly summary = computed(() => {
    const search = this.search();

    return `${search.make.name}, model year ${search.year}, ${search.vehicleType ?? 'all vehicle types'}`;
  });

  protected readonly firstRow = linkedSignal({
    source: this.visibleModels,
    computation: () => 0,
  });
}
