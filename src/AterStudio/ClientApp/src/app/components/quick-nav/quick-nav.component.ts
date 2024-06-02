import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Route, Router } from '@angular/router';

@Component({
  selector: 'app-quick-nav',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './quick-nav.component.html',
  styleUrl: './quick-nav.component.scss'
})
export class QuickNavComponent {
  showMore = false;

  constructor(
    private router: Router,
    private Route: ActivatedRoute
  ) {
  }
  toggle(): void {
    this.showMore = !this.showMore;
  }

  goToNote(): void {
    this.router.navigate(['/note/index']);
  }
}
