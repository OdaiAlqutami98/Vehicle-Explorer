import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Make } from '../interfaces/make.interface';
import { VehicleType } from '../interfaces/vehicle-type.interface';
import { VehicleModel } from '../interfaces/vehicle-model.interface';
import { GetModelsRequest } from '../interfaces/get-models-request.interface';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private readonly http = inject(HttpClient);

  getMakes(): Observable<Make[]> {
    return this.http.get<Make[]>('/api/makes/GetMakes');
  }

  getVehicleTypes(makeId: number): Observable<VehicleType[]> {
    const params = new HttpParams().set('makeId', makeId);

    return this.http.get<VehicleType[]>('/api/makes/GetVehicleTypes', { params });
  }

  getModels(request: GetModelsRequest): Observable<VehicleModel[]> {
    let params = new HttpParams().set('makeId', request.makeId).set('year', request.year);

    if (request.vehicleType != null) {
      params = params.set('vehicleType', request.vehicleType);
    }

    return this.http.get<VehicleModel[]>('/api/makes/GetModels', { params });
  }
}
