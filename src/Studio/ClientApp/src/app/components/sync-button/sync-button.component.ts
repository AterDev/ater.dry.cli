import { Component, EventEmitter, Input, Output } from '@angular/core';

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
}
