import { Component, input, output } from '@angular/core';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-load-error',
  imports: [Button, Message],
  templateUrl: './load-error.html',
  styleUrl: './load-error.scss',
})
export class LoadError {
  readonly message = input.required<string>();
  readonly retry = output<void>();
}
