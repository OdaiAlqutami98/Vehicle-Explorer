import { Component } from '@angular/core';
import { Skeleton } from 'primeng/skeleton';

@Component({
  selector: 'app-results-skeleton',
  imports: [Skeleton],
  templateUrl: './results-skeleton.html',
  styleUrl: './results-skeleton.scss',
})
export class ResultsSkeleton {
  protected readonly rows = [1, 2, 3, 4, 5, 6];
}
