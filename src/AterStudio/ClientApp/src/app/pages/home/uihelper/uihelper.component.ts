import { Component } from '@angular/core';

@Component({
  selector: 'app-uihelper',
  templateUrl: './uihelper.component.html',
  styleUrls: ['./uihelper.component.css']
})
export class UihelperComponent {
  input: string | null = null;
  editorOptions = {
    theme: 'vs-dark', language: 'csharp', minimap: {
      enabled: false
    }
  };

  editorHtmlOptions = {
    theme: 'vs-dark', language: 'html', minimap: {
      enabled: false
    }
  };
  editorTSOptions = {
    theme: 'vs-dark', language: 'html', minimap: {
      enabled: false
    }
  };

  generate(type: string): void {

  }
}
