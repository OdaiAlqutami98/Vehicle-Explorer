import { ModelSearch } from '../interfaces/model-search.interface';
import { VehicleModel } from '../interfaces/vehicle-model.interface';

export type LoadStatus = 'idle' | 'loading' | 'loaded' | 'error';

export interface OptionsState<T> {
  status: LoadStatus;
  options: T[];
  errorMessage?: string;
}

export type ResultsState =
  | { status: 'idle' }
  | { status: 'loading'; search: ModelSearch }
  | { status: 'loaded'; search: ModelSearch; models: VehicleModel[] }
  | { status: 'error'; search: ModelSearch; message: string };
