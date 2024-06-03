import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { StringComponent } from 'src/app/pages/tools/string/string.component';

@Component({
  selector: 'app-quick-nav',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatTooltipModule],
  templateUrl: './quick-nav.component.html',
  styleUrl: './quick-nav.component.scss'
})
export class QuickNavComponent {
  showMore = false;
  dialogRef!: MatDialogRef<{}, any>;
  constructor(
    private dialog: MatDialog,
    private router: Router,
    private Route: ActivatedRoute
  ) {
  }
  toggle(): void {
    this.showMore = !this.showMore;
  }

  openTool(name: string): void {
    switch (name) {
      case 'string':
        this.dialogRef = this.dialog.open(StringComponent, {
          minWidth: '400px',
        });
        break;
    }
    this.showMore = false;
  }
}
