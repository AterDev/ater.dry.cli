import { HttpEvent, HttpEventType } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatSelectionListChange } from '@angular/material/list';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, ActivatedRoute } from '@angular/router';
import { AdvanceService } from 'src/app/share/services/advance.service';


import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';
export enum ToolType {
  Entity = 0,
  Image = 1,
  Answer = 2
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
  answerContent: string = '';
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
      this.answerContent += "\n";
      this.isProcessing = true;
      switch (this.selectedTool) {
        case ToolType.Entity:
          this.generateEntity();
          break;
        case ToolType.Image:
          this.generatorImage();
          break;
        case ToolType.Answer:
          this.generatorAnswer();
          break;
      }
    } else {
      this.snb.open('请输入内容');
    }
  }

  generatorAnswer(): void {
    const url = this.service.baseUrl + `/api/Advance/answer?content=${this.content}`;
    const self = this;
    fetch(url, { method: 'GET' }).then((response) => {
      if (!response.ok) {
        this.snb.open('请求失败');
      }
      if (response.body) {
        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        reader.read().then(function processText({ done, value }) {
          if (done) {
            self.isProcessing = false;
            return;
          }
          const res = decoder.decode(value);
          if (res) {
            self.answerContent += res;
          }
          reader.read().then(processText);
        });
      }
    });
  }

  generatorImage(): void {
    this.service.getImages(this.content)
      .subscribe({
        next: (res) => {
          const imageUrl = res[0];
          const imgBLock = `![img](${imageUrl})
          `;
          this.answerContent += imgBLock;
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isProcessing = false;
        },
        complete: () => {
          this.isProcessing = false;
        }
      });
  }
  generateEntity(): void {
    const url = this.service.baseUrl + `/api/Advance/generateEntity?content=${this.content}`;
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
            self.isProcessing = false;
            return;
          }
          const res = decoder.decode(value);
          if (res) {
            self.answerContent += res;
          }
          reader.read().then(processText);
        });
      }
    });
  }

}
