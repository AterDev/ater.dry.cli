import { DialogRef } from '@angular/cdk/dialog';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Route, Router } from '@angular/router';
import { Project } from 'src/app/share/models/project/project.model';
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
  constructor(
    private service: ProjectService,
    public dialog: MatDialog,
    public snb: MatSnackBar,
    public router: Router,
    public route: ActivatedRoute
  ) {

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
        this.snb.open('未选择有效的sln文件');
        return;
      }
      this.service.add(name, path)
        .subscribe(res => {
          if (res) {
            this.snb.open('添加成功');
            this.dialogRef.close();
          }
        })
    } else {
      this.snb.open('输入有误，请查检');
    }
  }
  goToWorkspace(id: number): void {
    this.router.navigate(['/workspace', id]);
  }
  getProjects(): void {
    this.service.list()
      .subscribe(res => {
        this.projects = res;
      });
  }
}
