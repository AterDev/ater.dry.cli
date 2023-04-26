import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TemplateFile } from 'src/app/share/models/project/template-file.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/share/services/project.service';

@Component({
  selector: 'app-setting',
  templateUrl: './setting.component.html',
  styleUrls: ['./setting.component.css']
})
export class SettingComponent implements OnInit {
  isLoading = true;
  projectId: string;
  templateFile: TemplateFile | null = null;
  allFiles: TemplateFile[] = [];
  editorOptions = {
    theme: 'vs-dark', language: 'typescript', minimap: {
      enabled: false
    }
  };

  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    private snb: MatSnackBar
  ) {
    this.projectId = projectState.project!.id;
  }

  ngOnInit(): void {
    this.getFiles();
  }

  selectFile(name: string): void {
    this.service.getTemplateFile(this.projectId, name)
      .subscribe({
        next: (res) => {
          if (res) {
            this.templateFile = res;
          } else {
            this.snb.open('获取失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  getFiles(): void {
    this.service.getTemplateFiles(this.projectId)
      .subscribe({
        next: (res) => {
          if (res) {
            this.allFiles = res;
          } else {
            this.snb.open('');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  save(): void {
    if (this.templateFile !== null) {
      this.service.saveTemplateFile(this.projectId, {
        name: this.templateFile.name,
        content: this.templateFile.content
      }).subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('保存成功');
          } else {
            this.snb.open('');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
    }
  }
}
