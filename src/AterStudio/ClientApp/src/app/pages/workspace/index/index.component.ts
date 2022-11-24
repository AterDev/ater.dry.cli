import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { EntityFile } from 'src/app/share/models/entity/entity-file.model';
import { GenerateDto } from 'src/app/share/models/entity/generate-dto.model';
import { CommandType } from 'src/app/share/models/enum/command-type.model';
import { RequestLibType } from 'src/app/share/models/enum/request-lib-type.model';
import { EntityService } from 'src/app/share/services/entity.service';
@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
  RequestLibType = RequestLibType;
  CommandType = CommandType;
  projectId: string | null = null;
  entityFiles = [] as EntityFile[];
  columns: string[] = ['name', 'path', 'actions'];
  dataSource!: MatTableDataSource<EntityFile>;
  isLoading = true;
  requestForm!: FormGroup;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild("requestDialog", { static: true })
  requestTmpRef!: TemplateRef<{}>;
  @ViewChild("syncDialog", { static: true })
  syncTmpRef!: TemplateRef<{}>;

  constructor(
    public route: ActivatedRoute,
    public router: Router,
    public service: EntityService,
    public dialog: MatDialog,
    public snb: MatSnackBar
  ) {
    this.route.paramMap.subscribe(res => {
      this.projectId = res.get('id');
    })
  }
  ngOnInit(): void {
    this.initForm();
    this.getEntity();
  }

  initForm(): void {
    this.requestForm = new FormGroup({
      swagger: new FormControl<string | null>(null, []),
      type: new FormControl<RequestLibType>(RequestLibType.NgHttp, []),
      path: new FormControl<string | null>(null, [Validators.required])
    });
  }

  getEntity(): void {
    this.service.list(parseInt(this.projectId!))
      .subscribe(res => {
        if (res) {
          this.entityFiles = res;
          this.dataSource = new MatTableDataSource<EntityFile>(this.entityFiles);
        }
        this.isLoading = false;

      })
  }

  edit(path: string): void {

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

  generate(path: string, type: CommandType): void {
    const dto: GenerateDto = {
      projectId: parseInt(this.projectId!),
      entityPath: path,
      commandType: type
    };
    this.service.generate(dto)
      .subscribe(res => {
        if (res) {
          this.snb.open('生成成功');
        }
      })
  }

  generateRequest(): void {
    const type = this.requestForm.get('type')?.value as number;
    const path = this.requestForm.get('path')?.value as string;
    this.service.generateRequest(parseInt(this.projectId!), path, type)
      .subscribe(res => {
        if (res) {
          this.snb.open('生成成功');
          this.dialogRef.close();
        }
      })
  }

  generateSync(): void {
    this.service.generateSync(parseInt(this.projectId!))
      .subscribe(res => {
        if (res) {
          this.snb.open('同步前端成功');
          this.dialogRef.close();
        }
      })
  }

}
