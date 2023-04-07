import { DialogRef } from '@angular/cdk/dialog';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { Project } from 'src/app/share/models/project/project.model';
import { ProjectStateService } from 'src/app/share/project-state.service';
import { ProjectService } from 'src/app/share/services/project.service';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {

  @ViewChild("addDialog", { static: true })
  dialogTmpRef!: TemplateRef<{}>;
  dialogRef!: MatDialogRef<{}, any>;
  projects = [] as Project[];
  addForm!: FormGroup;
  type: string | null = null;
  constructor(
    private service: ProjectService,
    private projectState: ProjectStateService,
    public dialog: MatDialog,
    public snb: MatSnackBar,
    public router: Router,
    public route: ActivatedRoute
  ) {
    this.type = localStorage.getItem('type');
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
    this.dialogRef.afterClosed().subscribe(res => {

    });
  }

  addProject(): void {
    if (this.addForm.valid) {
      const name = this.addForm.get('displayName')?.value as string;
      const path = this.addForm.get('path')?.value as string;
      if (!path.endsWith('.sln')) {
        this.snb.open('路径不是有效的sln文件');
      }
      this.service.add(name, path)
        .subscribe(res => {
          if (res) {
            this.snb.open('添加成功');
            this.dialogRef.close();
            this.getProjects();
          }
        })
    } else {
      this.snb.open('输入有误，请查检');
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
  selectProject(id: string): void {
    const project = this.projects.find(p => p.id == id);
    console.log(project);
    if (project) {

      this.projectState.setProject(project);
      this.router.navigateByUrl('/workspace/code');

    } else {
      this.snb.open('无效的项目');
    }
  }
  getProjects(): void {
    this.service.list()
      .subscribe(res => {
        this.projects = res;
      });
  }
}
