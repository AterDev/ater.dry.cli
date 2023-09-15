import { Component } from '@angular/core';
import { MatSelectionListChange } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';
import { AdvanceService } from 'src/app/share/services/advance.service';

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
  content: string | null = null;
  selectedTool: ToolType;
  constructor(
    private snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private service: AdvanceService
  ) {
    this.selectedTool = ToolType.Entity;
  }

  ngOnInit() {
  }

  getData(): void { }


  selectTool(event: MatSelectionListChange): void {
    this.selectedTool = event.options[0].value;
  }


  send(): void {
    if (this.content != null && this.content != "") {
      this.isProcessing = true;
      this.service.generateEntity(this.content)
        .subscribe({
          next: (res) => {
            if (res) {
              console.log(res);

            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isProcessing = false;
          },
          complete: () => {
            this.isProcessing = false;
          }
        });
    } else {
      this.snb.open('请输入内容');
    }

  }
}
