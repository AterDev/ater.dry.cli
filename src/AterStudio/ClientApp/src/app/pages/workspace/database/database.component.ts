import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/share/services/project.service';

import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';

@Component({
  selector: 'app-database',
  templateUrl: './database.component.html',
  styleUrls: ['./database.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class DatabaseComponent implements OnInit {
  isLoding = true;
  content: string | null = null;
  projectId: string | null = null;
  editorOptions = { theme: 'vs-dark', language: 'markdown', minimap: { enabled: false } };
  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    private snb: MatSnackBar
  ) {

    this.projectId = projectState.project?.id!;
  }

  ngOnInit(): void {
    this.getContent();
  }
  getContent(): void {
    if (this.projectId)
      this.service.getDatabaseContent(this.projectId)
        .subscribe({
          next: (res) => {
            this.content = res;
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isLoding = false;
          },
          complete: () => {
            this.isLoding = false;
          }
        });
  }
}
