import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [CommonModule, MatCardModule],
  templateUrl: './index.component.html',
  styleUrl: './index.component.css'
})
export class IndexComponent {

}
