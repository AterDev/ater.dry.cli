import { Component } from '@angular/core';
import { MatSelectionListChange } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';

export enum ToolType {
  Entity = 0,
  Image = 1
}

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent {
  isLoading = true;
  isProcessing = false;
  ToolType = ToolType;

  selectedTool: ToolType;
  constructor(
    private snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
  ) {
    this.selectedTool = ToolType.Entity;
  }

  ngOnInit() {
  }

  getData(): void { }


  selectTool(event: MatSelectionListChange): void {

  }
}
