import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { SolutionType } from 'src/app/share/models/enum/solution-type.model';
import { ConfigOptions } from 'src/app/share/models/project/config-options.model';
import { Project } from 'src/app/share/models/project/project.model';
import { UpdateConfigOptionsDto } from 'src/app/share/models/project/update-config-options-dto.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/share/services/project.service';

import 'prismjs/plugins/line-numbers/prism-line-numbers.js';
import 'prismjs/components/prism-markup.min.js';
import 'prismjs/components/prism-csharp.min.js';
import { AdvanceService } from 'src/app/share/services/advance.service';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
  @ViewChild("addDialog", { static: true }) dialogTmpRef!: TemplateRef<{}>;
  @ViewChild("settingDialog", { static: true }) settingTmpRef!: TemplateRef<{}>;
  @ViewChild("updateProjectDialog", { static: true }) updateTmpRef!: TemplateRef<{}>;
  @ViewChild("addOpenAiKeyDialog", { static: true }) openAITmpRef!: TemplateRef<{}>;
  SolutionType = SolutionType;
  dialogRef!: MatDialogRef<{}, any>;
  projects = [] as Project[];
  current: Project | null = null;
  config: ConfigOptions | null = null;
  addForm!: FormGroup;
  settingForm!: FormGroup;
  type: string | null = null;
  isLoading = true;
  isProcessing = false;
  isUpdating = false;
  updated = false;
  updateResult: string | null = null;
  version: string | null;
  openAIKey: string | null = null;

  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    private advance: AdvanceService,
    public dialog: MatDialog,
    public snb: MatSnackBar,
    public router: Router,
    public route: ActivatedRoute
  ) {
    this.type = localStorage.getItem('type');
    this.version = projectState.version;
  }

  ngOnInit(): void {
    this.initForm();
    this.getProjects();
    this.getOpenAIKey();
  }

  initForm(): void {
    this.addForm = new FormGroup({
      displayName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]),
      path: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(100)])
    })
  }

  needUpdate(currentVersion: string | null): boolean {
    const majorVersion = this.version?.split('.')[0];
    const currentMajorVersion = currentVersion?.split('.')[0];
    if (majorVersion == null || currentMajorVersion == null) return false;
    return majorVersion !== currentMajorVersion;
  }

  getOpenAIKey(): void {
    this.advance.getConfig("openAIKey")
      .subscribe({
        next: (res) => {
          if (res) {
            this.openAIKey = res.value;
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

  addProjectDialog(): void {
    this.dialogRef = this.dialog.open(this.dialogTmpRef, {
      minWidth: 360
    });
  }

  addProject(): void {
    if (this.addForm.valid) {
      const name = this.addForm.get('displayName')?.value as string;
      const path = this.addForm.get('path')?.value as string;
      this.service.add(name, path)
        .subscribe({
          next: (res) => {
            this.snb.open('添加成功');
            this.dialogRef.close();
            this.addForm.reset();
            this.getProjects();
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        });
    } else {
      this.snb.open('输入有误，请查检');
    }
  }

  openSolution(path: string): void {
    this.service.openSolution(path)
      .subscribe({
        next: (res) => {
          if (res) {
          } else {
            this.snb.open('打开失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        }
      });
  }

  openSetting(project: Project): void {
    this.current = project;
    this.projectState.setProject(project);
    this.isProcessing = true;
    this.service.getConfigOptions()
      .subscribe({
        next: (res) => {
          if (res) {
            this.config = res;
            this.buildSettingForm();
            this.dialogRef = this.dialog.open(this.settingTmpRef, {
              minWidth: 450,
            })
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
  }

  openUpdate(project: Project): void {
    this.current = project;
    this.projectState.setProject(project);
    this.dialogRef = this.dialog.open(this.updateTmpRef, {
      minWidth: 450,
      hasBackdrop: true,
      closeOnNavigation: false,
      disableClose: true
    })
  }

  openOpenAI(): void {
    this.dialogRef = this.dialog.open(this.openAITmpRef, {
      minWidth: 450,
      hasBackdrop: true,
      closeOnNavigation: false,
      disableClose: true
    })
  }

  buildSettingForm(): void {
    this.settingForm = new FormGroup({
      dtoPath: new FormControl(this.config?.dtoPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      entityPath: new FormControl(this.config?.entityPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      entityFrameworkPath: new FormControl(this.config?.dbContextPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      storePath: new FormControl(this.config?.applicationPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      apiPath: new FormControl(this.config?.apiPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      idType: new FormControl(this.config?.idType ?? "Guid", [Validators.required, Validators.minLength(2), Validators.maxLength(20)]),
      isSplitController: new FormControl<boolean>(this.config?.isSplitController ?? false)
    });
  }
  saveSetting(): void {
    if (this.settingForm.valid && this.current) {
      const idType = this.settingForm.get('idType')?.value;
      if (idType !== 'Guid' && idType !== 'int') {
        this.snb.open('Id类型仅支持Guid或int');
        return;
      }
      this.isProcessing = true;
      const data = this.settingForm.value as UpdateConfigOptionsDto;

      this.service.updateConfig(data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('保存成功');
              this.dialogRef.close();
            } else {
              this.snb.open('保存失败');
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
      console.log(this.settingForm.errors);
    }
  }

  addOpenAiKey(): void {
    if (this.openAIKey) {
      this.advance.setConfig("openAIKey", this.openAIKey)
        .subscribe({
          next: (res) => {
            this.snb.open('保存成功');
            this.dialogRef.close();
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        });
    }
  }
  deleteConfirm(item: string): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      hasBackdrop: true,
      disableClose: false,
      minWidth: 300,
      data: {
        title: '删除项目',
        content: '将项目从工具中删除(不会删除本地文件)，是否确定?'
      }
    });

    ref.afterClosed().subscribe(res => {
      if (res) {
        this.delete(item);
      }
    });
  }

  delete(id: string): void {
    if (id) {
      this.service.delete(id)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('删除成功');
              this.getProjects();
            } else {
              this.snb.open('删除失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        })
    }
  }

  updateProject(): void {
    this.isUpdating = true;
    this.updated = false;

    this.service.updateSolution()
      .subscribe({
        next: (res) => {
          this.isUpdating = false;
          this.updated = true;
          this.updateResult = res;
          this.dialogRef.afterClosed().subscribe(_ => {
            this.updated = false;
            this.getProjects();
          });
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isUpdating = false;
          this.updated = true;
        },
        complete: () => {
          this.isUpdating = false;
          this.updated = true;
        }
      });
  }
  selectProject(id: string): void {
    const project = this.projects.find(p => p.id == id);
    if (project) {
      this.projectState.setProject(project);
      this.router.navigateByUrl('/workspace/index');

    } else {
      this.snb.open('无效的项目');
    }
  }
  getProjects(): void {
    this.isLoading = true;
    this.service.list()
      .subscribe({
        next: (res) => {
          if (res) {
            this.projects = res;
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
