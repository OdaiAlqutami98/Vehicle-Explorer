import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, catchError, map, of, startWith, switchMap } from 'rxjs';
import { OptionsState, ResultsState } from '../../state/explorer-state';
import { ModelSearch } from '../../interfaces/model-search.interface';
import { ResultsSkeleton } from '../results-skeleton/results-skeleton';
import { FilterCard } from '../filter-card/filter-card';
import { ResultsCard } from '../results-card/results-card';
import { VehicleService } from '../../services/vehicle.service';
import { Make } from '../../interfaces/make.interface';
import { VehicleType } from '../../interfaces/vehicle-type.interface';
import { LoadError } from '../../../../shared/components/load-error/load-error';
import { apiErrorMessage } from '../../../../shared/utils/api-error-message';

const FIRST_SUPPORTED_YEAR = 1995;

@Component({
  selector: 'app-explorer-page',
  imports: [FilterCard, LoadError, ResultsCard, ResultsSkeleton],
  templateUrl: './explorer-page.html',
  styleUrl: './explorer-page.scss',
})
export class ExplorerPage {
  private readonly api = inject(VehicleService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly years = buildYears();
  protected readonly makes = signal<OptionsState<Make>>({ status: 'loading', options: [] });
  protected readonly vehicleTypes = signal<OptionsState<VehicleType>>({
    status: 'idle',
    options: [],
  });

  protected readonly selectedMake = signal<Make | null>(null);
  protected readonly selectedYear = signal<number | null>(null);
  protected readonly selectedVehicleType = signal<string | null>(null);
  protected readonly results = signal<ResultsState>({ status: 'idle' });
  private readonly makeSelections = new Subject<Make | null>();
  private readonly searches = new Subject<ModelSearch | null>();

  constructor() {
    this.loadMakes();

    this.makeSelections
      .pipe(
        switchMap((make) =>
          make
            ? this.loadVehicleTypes(make)
            : of<OptionsState<VehicleType>>({ status: 'idle', options: [] }),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((state) => this.vehicleTypes.set(state));

    this.searches
      .pipe(
        switchMap((search) =>
          search ? this.loadModels(search) : of<ResultsState>({ status: 'idle' }),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((state) => this.results.set(state));
  }

  protected loadMakes(): void {
    this.api
      .getMakes()
      .pipe(
        map((options): OptionsState<Make> => ({ status: 'loaded', options })),
        catchError((error) =>
          of<OptionsState<Make>>({
            status: 'error',
            options: [],
            errorMessage: apiErrorMessage(error),
          }),
        ),
        startWith<OptionsState<Make>>({ status: 'loading', options: [] }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((state) => this.makes.set(state));
  }

  protected onMakeChange(make: Make | null): void {
    this.selectedMake.set(make);
    this.selectedVehicleType.set(null);
    this.makeSelections.next(make);
    this.searches.next(null);
  }

  protected showModels(): void {
    const make = this.selectedMake();
    const year = this.selectedYear();

    if (make === null || year === null) {
      return;
    }

    this.searches.next({ make, year, vehicleType: this.selectedVehicleType() });
  }

  protected searchAgain(search: ModelSearch): void {
    this.searches.next(search);
  }

  protected clearVehicleTypeAndSearch(): void {
    this.selectedVehicleType.set(null);
    this.showModels();
  }

  private loadVehicleTypes(make: Make): Observable<OptionsState<VehicleType>> {
    return this.api.getVehicleTypes(make.id).pipe(
      map((options): OptionsState<VehicleType> => ({ status: 'loaded', options })),
      catchError(() => of<OptionsState<VehicleType>>({ status: 'error', options: [] })),
      startWith<OptionsState<VehicleType>>({ status: 'loading', options: [] }),
    );
  }

  private loadModels(search: ModelSearch): Observable<ResultsState> {
    return this.api
      .getModels({
        makeId: search.make.id,
        year: search.year,
        vehicleType: search.vehicleType,
      })
      .pipe(
        map((models): ResultsState => ({
          status: 'loaded',
          search,
          models,
        })),

        catchError((error) =>
          of<ResultsState>({ status: 'error', search, message: apiErrorMessage(error) }),
        ),
        startWith<ResultsState>({ status: 'loading', search }),
      );
  }
}

/** From next year (new models are published early) down to the first supported year. */
function buildYears(): number[] {
  const years: number[] = [];

  for (let year = new Date().getFullYear() + 1; year >= FIRST_SUPPORTED_YEAR; year--) {
    years.push(year);
  }

  return years;
}
