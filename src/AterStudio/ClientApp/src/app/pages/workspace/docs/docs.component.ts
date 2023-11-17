import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { ApiDocTag } from 'src/app/share/models/api-doc-tag.model';
import { ApiDocInfo } from 'src/app/share/models/api-doc/api-doc-info.model';
import { CreateUIComponentDto } from 'src/app/share/models/api-doc/create-uicomponent-dto.model';
import { NgComponentInfo } from 'src/app/share/models/api-doc/ng-component-info.model';
import { EntityInfo } from 'src/app/share/models/entity-info.model';
import { ComponentType } from 'src/app/share/models/enum/component-type.model';
import { LanguageType } from 'src/app/share/models/enum/language-type.model';
import { OperationType } from 'src/app/share/models/enum/operation-type.model';
import { RequestLibType } from 'src/app/share/models/enum/request-lib-type.model';
import { UIType } from 'src/app/share/models/enum/uitype.model';
import { ConfigOptions } from 'src/app/share/models/project/config-options.model';
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
  ComponentType = ComponentType;
  LanguageType = LanguageType;
  UIType = UIType;
  project = {} as Project;
  projectId: string;
  isRefresh = false;
  isSync = false;
  isLoading = true;
  isOccupying = false;
  currentApi: RestApiInfo | null = null;
  currentModel: EntityInfo | null = null;
  selectedModel: EntityInfo | null = null;
  searchKey: string | null = null;
  modelSearchKey: string | null = null;
  /**
   * 文档列表
   */
  docs = [] as ApiDocInfo[];
  currentDoc: ApiDocInfo | null = null;
  newDoc = {} as ApiDocInfo;
  addForm!: FormGroup;
  editForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  requestForm!: FormGroup;
  clientRequestForm!: FormGroup;
  uiType: UIType = UIType.AngularMaterial;
  componentCodes: NgComponentInfo | null = null;

  @ViewChild("addDocDialog", { static: true })
  addTmpRef!: TemplateRef<{}>;

  @ViewChild("editDocDialog", { static: true })
  editTmpRef!: TemplateRef<{}>;

  @ViewChild("modelInfo", { static: true })
  modelTmpRef!: TemplateRef<{}>;

  @ViewChild("requestDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;

  @ViewChild("clientRequestDialog", { static: true })
  clientRequestTmpRef!: TemplateRef<{}>;
  config: ConfigOptions | null = null;
  restApiGroups = [] as RestApiGroup[];
  filterApiGroups = [] as RestApiGroup[];
  filterModelInfos = [] as EntityInfo[];
  modelInfos = [] as EntityInfo[];
  tags = [] as ApiDocTag[];
  tableColumns = ['name', 'type', 'requried', 'description'];
  modelTableColumns = ['name', 'type', 'requried', 'description', 'validator'];
  editorHtmlOptions = {
    theme: 'vs-dark', language: 'html', minimap: {
      enabled: false
    }
  };
  editorTSOptions = {
    theme: 'vs-dark', language: 'typescript', minimap: {
      enabled: false
    }
  };

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
    // get config options
    this.projectSrv.getConfigOptions()
      .subscribe({
        next: (res) => {
          if (res) {
            this.config = res;
            this.initForm();
          } else {
            this.snb.open('获取失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
    this.getDocs();
  }



  initForm(): void {
    // 添加表单
    this.addForm = new FormGroup({
      name: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(20)]),
      description: new FormControl<string | null>(null, [Validators.maxLength(100)]),
      path: new FormControl<string | null>('http://localhost:5002/swagger/name/swagger.json', [Validators.required, Validators.maxLength(200)]),
    });

    // 更新表单
    this.editForm = new FormGroup({
      name: new FormControl<string | null>(null, [Validators.required, Validators.maxLength(20)]),
      description: new FormControl<string | null>(null, [Validators.maxLength(100)]),
      path: new FormControl<string | null>('http://localhost:5002/swagger/name/swagger.json', [Validators.required, Validators.maxLength(200)]),
    });

    // 生成请求表单
    let defaultPath = `\\src\\app\\share`;

    if (this.projectState.project?.path?.endsWith(".sln")) {
      defaultPath = this.config?.rootPath! + '\\' + this.config?.apiPath + '\\ClientApp' + defaultPath;
    } else {
      defaultPath = this.config?.rootPath! + defaultPath;
    }

    this.requestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl<RequestLibType>(RequestLibType.NgHttp, []),
      path: new FormControl<string | null>(defaultPath, [Validators.required])
    });

    this.clientRequestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl<LanguageType>(LanguageType.CSharp, []),
      path: new FormControl<string | null>(this.config?.rootPath + '\\' + this.config?.apiPath ?? "", [Validators.required])
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
  export(): void {
    this.isSync = true;
    this.service.export(this.currentDoc!.id)
      .subscribe({
        next: (res) => {
          this.service.openFile(res, `${this.currentDoc?.name}.md`);
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isSync = false;
        },
        complete: () => {
          this.isSync = false;
        }
      });
  }

  openAddDocDialog(): void {
    this.dialogRef = this.dialog.open(this.addTmpRef, {
      minWidth: 400
    });
  }

  openEditDocDialog(): void {
    this.editForm.get('name')?.setValue(this.currentDoc?.name);
    this.editForm.get('description')?.setValue(this.currentDoc?.description);
    this.editForm.get('path')?.setValue(this.currentDoc?.path);
    this.dialogRef = this.dialog.open(this.editTmpRef, {
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

  openClientRequestDialog(): void {
    this.clientRequestForm.get('swagger')?.setValue(this.currentDoc?.path);
    this.dialogRef = this.dialog.open(this.clientRequestTmpRef, {
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

  editDoc(): void {
    if (this.editForm.valid) {
      const data = this.editForm.value as ApiDocInfo;
      data.projectId = this.projectId;
      if (this.currentDoc?.id) {
        this.service.update(this.currentDoc?.id, data)
          .subscribe(res => {
            if (res) {
              this.snb.open('更新成功');
              this.getDocs();
              this.editForm.reset();
              this.dialogRef.close();
            }
          });
      } else {
        this.snb.open('未选择接口文档');
      }

    }
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

  generateClientRequest(): void {
    this.isSync = true;
    const swagger = this.clientRequestForm.get('swagger')?.value as string;
    const type = this.clientRequestForm.get('type')?.value as number;
    const path = this.clientRequestForm.get('path')?.value as string;
    this.entitySrv.generateClientRequest(this.projectId!, path, type, swagger)
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

  generateUIComponent(type: ComponentType): void {
    if (this.currentModel) {
      this.isSync = true;
      const data: CreateUIComponentDto = {
        componentType: type,
        uiType: this.uiType,
        modelInfo: this.currentModel,
        serviceName: ''
      };
      // 获取到模型对应的服务名称
      data.serviceName = this.restApiGroups.find(g =>
        g.apiInfos?.find(api =>
          api.requestInfo?.id === this.currentModel?.id
          || api.responseInfo?.id === this.currentModel?.id))?.name ?? this.currentModel.name;

      this.service.createUIComponent(data)
        .subscribe({
          next: res => {
            if (res) {
              this.componentCodes = res;
              this.snb.open('生成成功');
            }
            this.isSync = false;
          },
          error: error => {
            this.snb.open(error.detail);
            this.isSync = false;
          }
        })
    } else {
      this.snb.open('未选择有效的模型');
    }
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
                this.filterModelInfos = this.modelInfos.filter(m => m.isEnum == false);
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

  filterModels(): void {
    if (this.modelSearchKey && this.modelSearchKey != null) {
      const modelSearchKey = this.modelSearchKey.toLowerCase();
      this.filterModelInfos = this.modelInfos.filter((val) => {
        return val.name?.toLowerCase().includes(modelSearchKey)
          || val.comment?.toLowerCase().includes(modelSearchKey)
          && val.isEnum == false
      });
    } else {
      this.filterModelInfos = this.modelInfos;
    }
  }

  selectApi(api: RestApiInfo): void {
    this.currentApi = api;
    console.log(this.currentApi);

  }
  selectModel(model: EntityInfo): void {
    this.currentModel = model;

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
