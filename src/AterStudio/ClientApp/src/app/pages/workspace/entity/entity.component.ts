import { Component } from '@angular/core';
import { Location } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { AdvanceService } from 'src/app/share/services/advance.service';
import { ProjectStateService } from 'src/app/share/project-state.service';

@Component({
  selector: 'app-entity',
  templateUrl: './entity.component.html',
  styleUrls: ['./entity.component.css']
})
export class EntityComponent {
  isProcessing = false;
  name: string | null = null;
  description: string | null = null;
  entities: string[] | null = null;
  selectedIndex: number | null = null;
  selectedContent: string | null = null;
  namespace: string | null = null;
  projectId: string | null = null;
  editorOptions = {
    theme: 'vs-dark', language: 'csharp', minimap: {
      enabled: false
    }
  };
  constructor(
    public snb: MatSnackBar,
    public router: Router,
    public service: AdvanceService,
    public projectState: ProjectStateService,
    private location: Location
  ) {
    if (projectState.project)
      this.projectId = projectState.project?.id;
  }
  ngOnInit(): void {

  }

  onInit(editor: any) {

  }
  generate(): void {
    if (this.name) {
      this.isProcessing = true;
      this.service.getEntity(this.name, this.description ?? null)
        .subscribe({
          next: (res) => {
            if (res) {
              this.entities = res;
            } else {
              this.snb.open('获取失败');
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
      this.snb.open('实体名称必填');
    }
  }
  select(index: number) {
    this.selectedIndex = this.selectedIndex === index ? null : index;
    this.selectedContent = this.entities![index];
  }

  save(): void {
    if (this.selectedContent && this.projectId) {
      this.isProcessing = true;
      this.service.createEntity(this.projectId, {
        namespace: this.namespace,
        content: this.selectedContent
      }).subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('创建成功,请在编辑器中查看');
          } else {
            this.snb.open('');
          }
          this.isProcessing = false;
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isProcessing = false;
        }
      });
    } else {
      this.snb.open('未选择内容或ProjectId丢失');
      console.log('projectId:', this.projectId);
    }
  }

  back(): void {
    this.location.back();

  }
}
