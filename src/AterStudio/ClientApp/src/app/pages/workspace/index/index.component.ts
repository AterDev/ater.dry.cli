import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { EntityFile } from 'src/app/share/models/entity/entity-file.model';
import { EntityService } from 'src/app/share/services/entity.service';
@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
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
      type: new FormControl<string | null>('NgHttp', []),
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
  generateRequest(): void {

  }


}
