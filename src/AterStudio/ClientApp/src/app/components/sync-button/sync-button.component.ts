import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'sync-button',
  templateUrl: './sync-button.component.html',
  styleUrl: './sync-button.component.css'
})
export class SyncButtonComponent {
  @Input() isSync: boolean;
  @Input() text: string = '';
  @Output() click: EventEmitter<void> = new EventEmitter();

  constructor() {
    this.isSync = false;
  }
  onClick() {
    this.click.emit();
  }
}
