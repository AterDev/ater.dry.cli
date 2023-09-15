import { HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatSelectionListChange } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
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
  answerContent: string | null = null;
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


  send() {
    if (this.content != null && this.content != "") {
      this.isProcessing = true;
      const url = this.service.baseUrl + `/api/Advance/generateEntity?content=${this.content ?? ''}`;
      const self = this;
      fetch(url, { method: 'POST' }).then((response) => {
        if (!response.ok) {
          this.snb.open('请求失败');
        }
        if (response.body) {
          const reader = response.body.getReader();
          const decoder = new TextDecoder('utf-8');
          reader.read().then(function processText({ done, value }) {
            if (done) {
              console.log('Stream finished');
              return;
            }
            const res = decoder.decode(value)
            self.answerContent += res;
            reader.read().then(processText);
          });
        }
      });

    } else {
      this.snb.open('请输入内容');
    }
  }
}
