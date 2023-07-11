import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { ConfigOptions } from 'src/app/share/models/project/config-options.model';
import { Project } from 'src/app/share/models/project/project.model';
import { UpdateConfigOptionsDto } from 'src/app/share/models/project/update-config-options-dto.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/share/services/project.service';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
  @ViewChild("addDialog", { static: true }) dialogTmpRef!: TemplateRef<{}>;
  @ViewChild("settingDialog", { static: true }) settingTmpRef!: TemplateRef<{}>;
  @ViewChild("updateProjectDialog", { static: true }) updateTmpRef!: TemplateRef<{}>;

  dialogRef!: MatDialogRef<{}, any>;
  projects = [] as Project[];
  current: Project | null = null;
  config: ConfigOptions | null = null;
  addForm!: FormGroup;
  settingForm!: FormGroup;
  type: string | null = null;
  isLoading = true;
  isProcessing = false;
  version: string | null;
  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
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
  }
  initForm(): void {
    this.addForm = new FormGroup({
      displayName: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]),
      path: new FormControl('', [Validators.required, Validators.minLength(3), Validators.maxLength(100)])
    })
  }
  addProjectDialog(): void {
    this.dialogRef = this.dialog.open(this.dialogTmpRef, {
      minWidth: 300
    });
  }

  addProject(): void {
    if (this.addForm.valid) {
      const name = this.addForm.get('displayName')?.value as string;
      const path = this.addForm.get('path')?.value as string;

      this.service.add(name, path)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('添加成功');
              this.dialogRef.close();
              this.addForm.reset();
              this.getProjects();
            } else {
              this.snb.open('添加失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          }
        });
    } else {
      this.snb.open('输入有误，请查检');
    }
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
    })
  }
  buildSettingForm(): void {
    this.settingForm = new FormGroup({
      dtoPath: new FormControl(this.config?.dtoPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      entityPath: new FormControl(this.config?.entityPath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
      storePath: new FormControl(this.config?.storePath, [Validators.required, Validators.minLength(1), Validators.maxLength(200)]),
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

  deleteConfirm(item: string): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      hasBackdrop: true,
      disableClose: false,
      data: {
        title: '删除',
        content: '是否确定删除?'
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
    this.isProcessing = true;
    this.service.updateSolution()
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open(res);
            this.dialogRef.close();
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
