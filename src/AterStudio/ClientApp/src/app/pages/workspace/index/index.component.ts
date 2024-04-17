import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSelectionListChange } from '@angular/material/list';
import { MatSelectChange } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { LoginService } from 'src/app/auth/login.service';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { BatchGenerateDto } from 'src/app/share/models/entity/batch-generate-dto.model';
import { EntityFile } from 'src/app/share/models/entity/entity-file.model';
import { GenerateDto } from 'src/app/share/models/entity/generate-dto.model';
import { CommandType } from 'src/app/share/models/enum/command-type.model';
import { ProjectType } from 'src/app/share/models/enum/project-type.model';
import { RequestLibType } from 'src/app/share/models/enum/request-lib-type.model';
import { ConfigOptions } from 'src/app/share/models/project/config-options.model';
import { Project } from 'src/app/share/models/project/project.model';
import { SubProjectInfo } from 'src/app/share/models/project/sub-project-info.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { EntityService } from 'src/app/share/services/entity.service';
import { ProjectService } from 'src/app/share/services/project.service';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
  RequestLibType = RequestLibType;
  CommandType = CommandType;
  project = {} as Project;
  projectId: string;
  entityFiles = [] as EntityFile[];
  baseEntityPath = '';
  columns: string[] = ['select', 'name', 'description', 'path', 'actions'];
  dataSource!: MatTableDataSource<EntityFile>;
  isLoading = true;
  requestForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  searchKey = '';
  isSync = false;
  force = false;
  isListening = false;
  isProcessing = false;
  modules: string[] = [];
  entityName: string | null = null;
  entityDescription: string | null = null;
  selectedService: string | null = null;
  @ViewChild("requestDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;
  @ViewChild("syncDialog", { static: true })
  syncTmpRef!: TemplateRef<{}>;
  @ViewChild("protobufDialog", { static: true }) protobufTmpRef!: TemplateRef<{}>;
  @ViewChild("apiDialog", { static: true }) apiTmpRef!: TemplateRef<{}>;
  @ViewChild("generateDialog", { static: true }) generateTmpRef!: TemplateRef<{}>;
  @ViewChild('previewDialog', { static: true }) previewTmpl!: TemplateRef<{}>;
  @ViewChild('ngPagesDialog', { static: true }) ngPagesTmpl!: TemplateRef<{}>;
  @ViewChild('addEntityDialog', { static: true }) addEntityTmpl!: TemplateRef<{}>;
  @ViewChild('contextMenu', { static: true }) contextMenuTmpl!: TemplateRef<{}>;
  @ViewChild('addDtoDialog', { static: true }) addDtoTmpl!: TemplateRef<{}>;
  @ViewChild('addsServiceDialog', { static: true }) addServiceTmpl!: TemplateRef<{}>;

  newServiceName: string | null = null;
  editorOptions = { theme: 'vs-dark', language: 'csharp', minimap: { enabled: false } };
  webPath: string | null = null;
  previewItem: EntityFile | null = null;
  newDtoFileName: string = '';
  newDtoDescription: string = '';

  selection = new SelectionModel<EntityFile>(true, []);
  selectedWebProjectIds: string[] = [];
  webProjects: SubProjectInfo[] = [];
  currentEntity: EntityFile | null = null;
  currentType: CommandType | null = null;
  isBatch = false;
  isCopied = false;
  isLogin = false;
  showModuleEntity = true;
  config: ConfigOptions | null = null;

  editor: any;
  constructor(
    public route: ActivatedRoute,
    public router: Router,
    public service: EntityService,
    public projectSrv: ProjectService,
    public projectState: ProjectStateService,
    public dialog: MatDialog,
    public snb: MatSnackBar,
    private loginService: LoginService
  ) {
    if (projectState.project) {
      this.project = projectState.project;
      this.projectId = projectState.project?.id;
    } else {
      this.projectId = '';
      this.router.navigateByUrl('/');
    }
    this.isLogin = loginService.isLogin;
  }

  ngOnInit(): void {
    this.projectSrv.getConfigOptions()
      .subscribe({
        next: (res) => {
          if (res) {
            this.config = res;
            this.initForm();
            this.getProjectInfo();

          } else {
            this.snb.open('获取失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
    this.getEntity();
    this.getProjects();
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  toggleAllRows() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }
    this.selection.select(...this.dataSource.data);
  }

  toggleModule() {
    this.showModuleEntity = !this.showModuleEntity;
    if (this.showModuleEntity) {
      this.dataSource.data = this.entityFiles;
    } else {
      this.dataSource.data = this.entityFiles.filter(e => e.module === null || e.module === '');
    }

  }
  displayContextMenu(event: any, entity: EntityFile): void {
    if (event.button === 2) {
      this.dialog.closeAll();
      this.dialogRef = this.dialog.open(this.contextMenuTmpl, {
        position: { top: `${event.pageY}px`, left: `${event.pageX}px` },
        disableClose: false,
        data: entity,
      });
    }
  }

  initEditor(editor: any): void {
    this.editor = editor;
  }

  getProjectInfo(): void {
    this.projectSrv.getConfigOptions()
      .subscribe(res => {
        if (res) {
          this.requestForm.get('path')?.setValue(res.webAppPath);
        }
      });
  }

  initForm(): void {
    this.requestForm = new FormGroup({
      swagger: new FormControl<string | null>('./swagger.json', []),
      type: new FormControl<RequestLibType>(RequestLibType.NgHttp, []),
      path: new FormControl<string | null>(null, [Validators.required])
    });

    let defaultPath = `\\src\\app\\pages`;
    if (this.projectState.project?.path?.endsWith(".sln")) {
      this.webPath = this.config?.rootPath! + '\\' + this.config?.apiPath + '\\ClientApp' + defaultPath;
    } else {
      this.webPath = this.config?.rootPath! + defaultPath;
    }
  }

  getEntity(): void {
    this.selection.clear();
    this.service.list(this.projectId!, this.selectedService ?? '')
      .subscribe(res => {
        if (res.length > 0) {
          this.entityFiles = res;
          this.baseEntityPath = res[0].baseDirPath ?? '';
          this.dataSource = new MatTableDataSource<EntityFile>(this.entityFiles);

          this.dataSource.filterPredicate = (data, filter: string) => {
            if (data.name) {
              return data.name.toLowerCase().indexOf(filter) > -1
                || data.module?.toLowerCase() === filter;
            }
            return false;
          }
        } else {
          this.dataSource = new MatTableDataSource<EntityFile>([]);
        }
        this.isLoading = false;
      })
  }

  clean(): void {
    this.isSync = true;
    this.service.cleanSolution()
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open(res);
            this.getEntity();
          }
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

  openRequestDialog(): void {
    this.dialogRef = this.dialog.open(this.requestTmpRef, {
      minWidth: 400
    });
  }
  openSyncDialog(): void {
    this.dialogRef = this.dialog.open(this.syncTmpRef, {
      minWidth: 300
    });
  }
  openNgPagesDialog(element: EntityFile | null): void {
    this.currentEntity = element;
    this.dialogRef = this.dialog.open(this.ngPagesTmpl, {
      minWidth: 400
    });
  }


  openAddDtoDialog(data: EntityFile): void {
    this.previewItem = data;
    this.newDtoFileName = data.name.replace('.cs', '') + 'Dto';
    this.dialog.closeAll();
    this.dialogRef = this.dialog.open(this.addDtoTmpl, {
      minWidth: 400,
    });
  }

  openWithVSCode(data: EntityFile): void {
    window.open(`vscode://file/${data.baseDirPath}${data.path}`);
    this.dialogRef.close();
  }

  clearCodesDialog(): void {
    this.dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: '确认清除？',
        content: '该操作将清除所有选中实体相关生成的代码，部分代码需要您手动删除！'
      }
    });

    this.dialogRef.afterClosed()
      .subscribe({
        next: (res) => {
          if (res) {
            let selected = this.selection.selected;
            if (selected.length > 0) {
              let data: BatchGenerateDto = {
                projectId: this.projectId!,
                entityPaths: selected.map(s => this.baseEntityPath + s.path),
                commandType: CommandType.Clear,
                force: this.force
              };
              this.isSync = true;
              this.service.batchGenerate(data)
                .subscribe({
                  next: (res) => {
                    if (res) {
                      this.snb.open('清理成功');
                      this.getEntity();
                    } else {
                      this.snb.open('清理失败');
                    }
                  },
                  error: (error) => {
                    this.snb.open(error.detail);
                    console.log(error.detail);

                    this.isSync = false;
                  },
                  complete: () => {
                    this.isSync = false;
                  }
                });
            } else {
              this.snb.open('未选择任何实体');
            }
          }
        }
      });
  }

  openAddEntity(): void {
    this.router.navigateByUrl('/workspace/entity');
  }

  filterEntity() {
    this.dataSource.filter = this.searchKey.trim().toLowerCase();
  }

  searchEntity(event: MatSelectChange) {
    const filterValue = event.value ?? '';
    this.selectedService = filterValue;
    this.getEntity();

  }
  async openPreviewDialog(item: EntityFile, isManager: boolean) {
    if (isManager) {
      item = await this.getFileContent(item.name!, true, item.module ?? '');
    }
    this.previewItem = item;
    this.dialogRef = this.dialog.open(this.previewTmpl, {
      minWidth: 850,
      minHeight: 800
    });
  }

  async getFileContent(entityName: string, isManager: boolean, moduleName: string | null): Promise<EntityFile> {
    return await lastValueFrom(this.service.getFileContent(entityName, isManager, moduleName));
  }

  copyCode(): void {
    this.isCopied = true;
    setTimeout(() => {
      this.isCopied = false;
    }, 1500);
  }

  addDto(open: boolean = false): void {
    if (this.previewItem?.baseDirPath && this.previewItem?.path) {
      const path = this.previewItem?.baseDirPath + this.previewItem?.path;
      this.service.createDto(path, this.newDtoFileName, this.newDtoDescription)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('添加成功');
              this.dialogRef.close();
              if (open) {
                window.open(`vscode://file/${res}`);
              }
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complete: () => {
          }
        });
    }
  }

  getProjects(): void {
    this.projectSrv.getAllProjectInfos(this.projectId)
      .subscribe({
        next: (res) => {
          if (res) {
            this.webProjects = res.filter(p => {
              return p.projectType == ProjectType.Web
                && !p.name?.endsWith('Test.csproj')
                && (p.path.includes('Microservice'));
            });

            // change webProjects name contont
            this.webProjects.forEach(p => {
              p.name = p.name.replace('.csproj', '');
            });

          } else {
            this.snb.open('没有有效的项目');
          }
        },
        error: (error) => {
          this.snb.open(error);
        }
      });
  }

  openInfo(url: string): void {
    window.open(url, '_blank');
  }
  openAddService(): void {
    this.dialogRef = this.dialog.open(this.addServiceTmpl, {
      minWidth: 400
    });
  }

  addService(): void {
    if (this.newServiceName) {
      this.projectSrv.addService(this.newServiceName)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('创建成功');
              this.dialogRef.close();
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            this.isSync = false;
          },
          complete: () => {
            this.isSync = false;
          }
        });

    } else {
      this.snb.open('请输入服务名称');
    }
  }

  openSelectProjectDialog(type: CommandType, element: EntityFile | null): void {
    this.currentEntity = element;
    this.currentType = type;
    switch (type) {
      case CommandType.API:
        this.dialogRef = this.dialog.open(this.apiTmpRef, {
          minWidth: 300
        });
        break;

      case CommandType.Protobuf:
        this.dialogRef = this.dialog.open(this.protobufTmpRef, {
          minWidth: 300
        });
        break;
      default:
        break;
    }
  }

  openGenerateDialog(type: CommandType, element: EntityFile | null): void {
    this.isBatch = element === null;
    this.currentEntity = element;
    this.currentType = type;
    this.dialogRef = this.dialog.open(this.generateTmpRef, {
      minWidth: 300,
    });
  }

  genNgModule(): void {
    if (this.currentEntity && this.webPath) {
      this.isSync = true;
      this.service.generateNgModule(this.projectId, this.currentEntity.name!, this.webPath)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('生成成功');
              this.dialogRef.close();
            } else {
              this.snb.open('生成失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            console.log(error.detail);
            this.isSync = false;
          },
          complete: () => {
            this.isSync = false;
          }
        });

    } else {
      this.snb.open('请填写路径');
    }
  }
  generate(): void {
    if (this.isBatch) {
      this.batch(this.currentType!);
    } else {
      if (this.currentEntity && this.currentType !== null) {
        this.isSync = true;
        const dto: GenerateDto = {
          projectId: this.projectId!,
          entityPath: this.baseEntityPath + this.currentEntity.path,
          commandType: this.currentType,
          serviceName: this.selectedService,
          force: this.force
        };
        this.service.generate(dto)
          .subscribe({
            next: (res) => {
              if (res) {
                this.snb.open('生成成功');
                this.dialogRef.close();
              } else {
                this.snb.open('');
              }
            },
            error: (error) => {
              this.isSync = false;
              this.snb.open(error.detail);
            },
            complete: () => {
              this.isSync = false;
            }
          });

      } else {
        this.snb.open('未选择任何实体');
      }
    }
  }
  batch(type: CommandType): void {
    let selected = this.selection.selected;
    if (this.currentEntity !== null) {
      selected = [this.currentEntity];
    }
    if (selected.length > 0) {
      let data: BatchGenerateDto = {
        projectId: this.projectId!,
        entityPaths: selected.map(s => this.baseEntityPath + s.path),
        commandType: type,
        serviceName: this.selectedService,
        force: this.force
      };
      // 参数
      if (this.selectedWebProjectIds.length > 0
        && (type == CommandType.Protobuf
          || type == CommandType.API)) {
        data.projectPath = this.selectedWebProjectIds;
      }
      this.isSync = true;
      this.service.batchGenerate(data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('生成成功');
              this.dialogRef.close();
            } else {
              this.snb.open('生成失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
            console.log(error.detail);
            this.isSync = false;
          },
          complete: () => {
            this.isSync = false;
          }
        });
    } else {
      this.snb.open('未选择任何实体');
    }
  }

  selectProject(event: MatSelectionListChange) {
    var data = event.source.selectedOptions.selected;
    this.selectedWebProjectIds = data.map<string>(d => d.value);
  }
  generateRequest(): void {
    this.isSync = true;
    const swagger = this.requestForm.get('swagger')?.value as string;
    const type = this.requestForm.get('type')?.value as number;
    const path = this.requestForm.get('path')?.value as string;
    this.service.generateRequest(this.projectId!, path, type, swagger)
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

  generateSync(): void {
    this.isSync = true;
    this.service.generateSync(this.projectId!)
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('同步前端成功');
            this.dialogRef.close();
          }
          this.isSync = false;
        },
        error: () => {
          this.isSync = false;
        }
      })
  }

  goToDto(entity: EntityFile): void {
    this.projectState.currentEntity = entity;
    this.router.navigate(['../dto', entity.name], { relativeTo: this.route });
  }
}
