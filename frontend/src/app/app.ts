import { Component } from '@angular/core';
import { ExplorerPage } from './features/vehicle-explorer/components/explorer-page/explorer-page';

@Component({
  selector: 'app-root',
  imports: [ExplorerPage],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
