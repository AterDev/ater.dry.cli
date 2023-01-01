import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { ApiDocTag } from 'src/app/share/models/api-doc-tag.model';
import { ApiDocInfo } from 'src/app/share/models/api-doc/api-doc-info.model';
import { EntityInfo } from 'src/app/share/models/entity-info.model';
import { OperationType } from 'src/app/share/models/enum/operation-type.model';
import { Project } from 'src/app/share/models/project/project.model';
import { RestApiGroup } from 'src/app/share/models/rest-api-group.model';
import { RestApiInfo } from 'src/app/share/models/rest-api-info.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ApiDocService } from 'src/app/share/services/api-doc.service';
import { ProjectService } from 'src/app/share/services/project.service';

@Component({
  selector: 'app-docs',
  templateUrl: './docs.component.html',
  styleUrls: ['./docs.component.css']
})
export class DocsComponent implements OnInit {
  OperationType = OperationType;
  project = {} as Project;
  projectId: string;
  isRefresh = false;
  isLoading = true;
  isOccupying = false;

  /**
   * 文档列表
   */
  docs = [] as ApiDocInfo[];
  currentDoc = {} as ApiDocInfo;
  newDoc = {} as ApiDocInfo;
  addForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild("addDocDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;

  restApiGroups = [] as RestApiGroup[];
  modelInfos = [] as EntityInfo[];
  tags = [] as ApiDocTag[];

  constructor(
    public projectSrv: ProjectService,
    public projectState: ProjectStateService,
    public service: ApiDocService,
    public router: Router,
    public dialog: MatDialog,
    public snb: MatSnackBar
  ) {
    if (projectState.project) {
      this.projectId = projectState.project?.id;
    } else {
      this.projectId = '';
      this.router.navigateByUrl('/');
    }
  }

  ngOnInit(): void {
    this.getDocs();
    this.initForm();
  }

  initForm(): void {
    this.addForm = new FormGroup({
      name: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(20)]),
      description: new FormControl<string | null>(null, [Validators.maxLength(100)]),
      path: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(200)]),
    });
  }
  getDocs(): void {
    this.service.list(this.projectId)
      .subscribe(res => {
        if (res) {
          this.docs = res;
          if (res.length > 0) {
            this.currentDoc = res[0];
            this.getDocContent();
          }
        }

      });
  }

  openAddDocDialog(): void {
    this.dialogRef = this.dialog.open(this.requestTmpRef, {
      minWidth: 400
    });
  }


  addDoc(): void {
    if (this.addForm.valid) {
      const data = this.addForm.value as ApiDocInfo;
      data.projectId = this.projectId;
      console.log(data);
      this.service.add(data)
        .subscribe(res => {
          if (res) {
            this.snb.open('添加成功');
            this.getDocs();
            this.addForm.reset();
            this.dialogRef.close();
          }
        });
    }
  }
  delete(): void {
    const id = this.currentDoc.id;
    if (id) {
      this.service.delete(id)
        .subscribe(res => {
          if (res) {
            this.snb.open('删除成功');
            this.getDocs();
          }
        })
    }
  }

  getDocContent(): void {
    const id = this.currentDoc.id;
    if (id) {
      this.service.getApiDocContent(id)
        .subscribe(res => {
          if (res) {
            this.restApiGroups = res.restApiGroups!;
            this.modelInfos = res.modelInfos!;
            this.tags = res.openApiTags!;
          }
          this.isLoading = false;
        })
    }
  }
  getApiTip(api: RestApiInfo): string {
    return `[${OperationType[api.operationType!]}] ${api.router}`;
  }

  getApiTypeColor(type: OperationType): string {
    switch (type) {
      case OperationType.Get:
        return '#61affe';

      case OperationType.Post:
        return '#49cc90';

      case OperationType.Put:
        return '#fca130';

      case OperationType.Delete:
        return '#f93e3e';
      default:
        return '#222222';
    }
  }

  refresh(): void { }
}