import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { ApiDocTag } from 'src/app/share/models/api-doc-tag.model';
import { ApiDocInfo } from 'src/app/share/models/api-doc/api-doc-info.model';
import { EntityInfo } from 'src/app/share/models/entity-info.model';
import { OperationType } from 'src/app/share/models/enum/operation-type.model';
import { RequestLibType } from 'src/app/share/models/enum/request-lib-type.model';
import { Project } from 'src/app/share/models/project/project.model';
import { PropertyInfo } from 'src/app/share/models/property-info.model';
import { RestApiGroup } from 'src/app/share/models/rest-api-group.model';
import { RestApiInfo } from 'src/app/share/models/rest-api-info.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ApiDocService } from 'src/app/share/services/api-doc.service';
import { EntityService } from 'src/app/share/services/entity.service';
import { ProjectService } from 'src/app/share/services/project.service';

@Component({
  selector: 'app-docs',
  templateUrl: './docs.component.html',
  styleUrls: ['./docs.component.css'],
})
export class DocsComponent implements OnInit {
  OperationType = OperationType;
  RequestLibType = RequestLibType;
  project = {} as Project;
  projectId: string;
  isRefresh = false;
  isSync = false;
  isLoading = true;
  isOccupying = false;
  currentApi: RestApiInfo | null = null;
  selectedModel: EntityInfo | null = null;
  searchKey: string | null = null;
  /**
   * 文档列表
   */
  docs = [] as ApiDocInfo[];
  currentDoc: ApiDocInfo | null = null;
  newDoc = {} as ApiDocInfo;
  addForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  requestForm!: FormGroup;

  @ViewChild("addDocDialog", { static: true })
  addTmpRef!: TemplateRef<{}>;

  @ViewChild("modelInfo", { static: true })
  modelTmpRef!: TemplateRef<{}>;

  @ViewChild("requestDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;

  restApiGroups = [] as RestApiGroup[];
  filterApiGroups = [] as RestApiGroup[];
  modelInfos = [] as EntityInfo[];
  tags = [] as ApiDocTag[];
  tableColumns = ['name', 'type', 'requried', 'description'];

  constructor(
    public projectSrv: ProjectService,
    public projectState: ProjectStateService,
    public service: ApiDocService,
    public entitySrv: EntityService,
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
    this.initForm();
    this.getDocs();
  }

  initForm(): void {
    this.addForm = new FormGroup({
      name: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(20)]),
      description: new FormControl<string | null>(null, [Validators.maxLength(100)]),
      path: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(200)]),
    });

    let defaultPath = `\\src\\app\\share`;

    if (this.projectState.project?.path?.endsWith(".sln")) {
      defaultPath = this.projectState.project.httpPath + '\\ClientApp' + defaultPath;
    } else {
      defaultPath = this.projectState.project?.path + defaultPath;
    }

    this.requestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl<RequestLibType>(RequestLibType.NgHttp, []),
      path: new FormControl<string | null>(defaultPath, [Validators.required])
    });
  }
  getDocs(): void {
    this.service.list(this.projectId)
      .subscribe({
        next: (res) => {
          if (res) {
            this.docs = res;
            if (res.length > 0) {
              this.currentDoc = res[0];
              this.getDocContent();
            } else {
              this.isLoading = false;
            }
          }

        },
        error: error => {
          this.isLoading = false;
          this.snb.open(error);
        }
      });
  }

  openAddDocDialog(): void {
    this.dialogRef = this.dialog.open(this.addTmpRef, {
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
  openRequestDialog(): void {
    this.requestForm.get('swagger')?.setValue(this.currentDoc?.path);
    this.dialogRef = this.dialog.open(this.requestTmpRef, {
      minWidth: 400
    });
  }
  delete(): void {
    const id = this.currentDoc!.id;
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

  edit(): void {

  }

  generateRequest(): void {
    this.isSync = true;
    const swagger = this.requestForm.get('swagger')?.value as string;
    const type = this.requestForm.get('type')?.value as number;
    const path = this.requestForm.get('path')?.value as string;
    this.entitySrv.generateRequest(this.projectId!, path, type, swagger)
      .subscribe({
        next: res => {
          if (res) {
            this.snb.open('生成成功');
            this.dialogRef.close();
          }
          this.isSync = false;
        },
        error: () => {
          this.isSync = false;
        }
      })
  }

  getDocContent(): void {
    const id = this.currentDoc!.id;
    if (id) {
      this.service.getApiDocContent(id)
        .subscribe(
          {
            next: res => {
              if (res) {
                this.restApiGroups = res.restApiGroups!;
                this.filterApiGroups = this.restApiGroups;
                this.modelInfos = res.modelInfos!;
                this.tags = res.openApiTags!;
                // 更新当前展示的内容
                if (this.currentApi != null) {
                  const updateContent = this.filterApiGroups
                    .map(g => g.apiInfos!)
                    .flat(1)
                    .find((a) => a?.router == this.currentApi?.router);

                  console.log(updateContent);

                  if (updateContent) {
                    this.currentApi = updateContent;
                  }
                }
              }
              this.isLoading = false;
            },
            error: error => {
              this.isLoading = false;
            }
          })
    }
  }

  filterApis(): void {
    if (this.searchKey && this.searchKey != null) {
      const searchKey = this.searchKey.toLowerCase();
      this.filterApiGroups = this.restApiGroups.filter((val) => {
        return val.name?.toLowerCase().includes(searchKey)
          || val.apiInfos!.findIndex((api) => {
            return api.router?.toLowerCase().includes(searchKey)
              || api.summary?.toLowerCase().includes(searchKey)
              || api.tag?.toLowerCase().includes(searchKey)
          }) > -1
      });

      console.log(this.filterApiGroups);

      for (let index = 0; index < this.filterApiGroups.length; index++) {
        const group = this.filterApiGroups[index];
        this.filterApiGroups[index].apiInfos = group.apiInfos!
          .filter((api) => {
            return api.router?.toLowerCase().includes(searchKey)
              || api.summary?.toLowerCase().includes(searchKey)
              || api.tag?.toLowerCase().includes(searchKey)
          })
      }
    } else {
      this.filterApiGroups = this.restApiGroups;
    }
  }

  selectApi(api: RestApiInfo): void {
    this.currentApi = api;
    console.log(this.currentApi);

  }

  showModel(prop: PropertyInfo): void {
    if (prop.isNavigation) {
      this.selectedModel = this.modelInfos.find(m => m.name == prop.navigationName) ?? null;
      console.log(this.selectedModel);
      if (this.selectedModel) {
        this.dialog.closeAll();
        this.dialog.open(this.modelTmpRef, {
          minWidth: 400
        });
      }
    }
  }

  getApiTip(api: RestApiInfo): string {
    return `[${OperationType[api.operationType!]}] ${api.router}`;
  }

  getApiTypeColor(type: OperationType): string {
    switch (type) {
      case OperationType.Get:
        return '#318deb';

      case OperationType.Post:
        return '#14cc78';

      case OperationType.Put:
        return '#fca130';

      case OperationType.Delete:
        return '#f93e3e';
      default:
        return '#222222';
    }
  }

  refresh(): void {
    this.isLoading = true;
    this.getDocContent();
  }
}
