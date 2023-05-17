import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { EntityService } from 'src/app/share/services/entity.service';
import { EntityFile } from 'models/entity/entity-file.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { MatTabChangeEvent } from '@angular/material/tabs';

@Component({
  selector: 'app-dto',
  templateUrl: './dto.component.html',
  styleUrls: ['./dto.component.css']
})
export class DtoComponent implements OnInit {
  name: string | null = null;
  path: string | null = null;
  dtos: EntityFile[] = [];
  isLoading = true;
  projectId: string;
  editorOptions = {
    theme: 'vs-dark', language: 'csharp', minimap: {
      enabled: false
    }
  };
  currentTabName: string | null = null;
  code: string = '';
  constructor(
    public route: ActivatedRoute,
    public snb: MatSnackBar,
    public projectState: ProjectStateService,
    private service: EntityService,
    private location: Location
  ) {
    this.name = this.route.snapshot.paramMap.get('name');

    if (projectState.project) {
      this.projectId = projectState.project?.id;
    } else {
      this.projectId = '';
    }
  }

  ngOnInit(): void {
    this.getDtos();
  }

  onInit(editor: any) {

  }
  getDtos(): void {
    if (this.projectId && this.name) {
      this.service.getDtos(this.name)
        .subscribe({
          next: (res) => {
            if (res) {
              this.dtos = res;
              if (this.dtos.length > 0) {
                this.code = this.dtos[0].content ?? '';
                this.currentTabName = this.dtos[0].name!;
              }
            } else {
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complete: () => {
            this.isLoading = false;
          }
        });
    }
  }

  tabChange(event: MatTabChangeEvent): void {
    var tab = event.tab.textLabel;
    this.currentTabName = event.tab.textLabel;
    this.code = this.dtos.find((val) => val.name == tab)?.content ?? '';
  }

  save(): void {
    if (this.currentTabName) {
      this.service.updateDtoContent(this.projectId,
        {
          fileName: this.currentTabName,
          content: this.code
        })
        .subscribe({
          next: (res) => {
            if (res) {
              this.dtos.find((val) => val.name == this.currentTabName)!.content = this.code;
              this.snb.open('保存成功');
            } else {
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        });
    } else {
      this.snb.open('未选择有效文件');
    }
  }
  back(): void {
    this.location.back();

  }
}
