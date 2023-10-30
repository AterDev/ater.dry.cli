import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ToolsService } from 'src/app/share/services/tools.service';

@Component({
  selector: 'app-json2-type',
  templateUrl: './json2-type.component.html',
  styleUrls: ['./json2-type.component.css']
})
export class Json2TypeComponent {
  editorOptions = { theme: 'vs-dark', language: 'csharp', minimap: { enabled: false } };
  jsonEditorOptions = { theme: 'vs-dark', language: 'json', minimap: { enabled: false } };
  jsonContent = '';
  classCode: string | null = null;
  editor: any;

  constructor(
    private service: ToolsService,
    private snb: MatSnackBar
  ) {
  }

  onInit(editor: any) {
    this.editor = editor;
  }

  ngOnInit(): void {

  }

  convert(): void {
    if (!this.jsonContent) {
      this.snb.open('请输入json内容', '关闭', { duration: 2000 });
      return;
    }


    this.service.convertToClass({
      content: this.jsonContent
    })
      .subscribe(res => {
        this.classCode = res.join('\n');
      });
  }

}
