import { Make } from './make.interface';

export interface ModelSearch {
  make: Make;
  year: number;
  vehicleType: string | null;
}
