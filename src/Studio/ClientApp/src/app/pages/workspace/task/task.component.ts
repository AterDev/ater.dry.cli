import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, forkJoin } from 'rxjs';
import { GenActionService } from 'src/app/services/gen-action/gen-action.service';
import { GenActionFilterDto } from 'src/app/services/gen-action/models/gen-action-filter-dto.model';
import { GenActionItemDtoPageList } from 'src/app/services/gen-action/models/gen-action-item-dto-page-list.model';
import { GenActionItemDto } from 'src/app/services/gen-action/models/gen-action-item-dto.model';
import { GenActionAddDto } from 'src/app/services/gen-action/models/gen-action-add-dto.model';
import { GenActionUpdateDto } from 'src/app/services/gen-action/models/gen-action-update-dto.model';

@Component({
  selector: 'app-index',
  templateUrl: './task.component.html',
  styleUrls: ['./task.component.scss']
})
export class TaskComponent implements OnInit {
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: GenActionItemDto[] = [];
  columns: string[] = ['name', 'description', 'actions'];
  dataSource!: MatTableDataSource<GenActionItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('addDialog', { static: true }) addTmpl!: TemplateRef<{}>;
  isEditable = false;
  addForm!: FormGroup;
  addDto: GenActionAddDto | null = null;
  currentItem = {} as GenActionItemDto;
  filter: GenActionFilterDto;
  pageSizeOption = [12, 20, 50];
  constructor(
    private service: GenActionService,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    this.filter = {
      pageIndex: 1,
      pageSize: 12,
      name: null
    };
  }

  get name(): FormControl { return this.addForm.get('name') as FormControl; }
  get description(): FormControl { return this.addForm.get('description') as FormControl; }

  ngOnInit(): void {
    forkJoin([this.getListAsync()])
      .subscribe({
        next: ([res]) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<GenActionItemDto>(this.data);
            }
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  getListAsync(): Observable<GenActionItemDtoPageList> {
    return this.service.filter(this.filter);
  }

  getList(event?: PageEvent): void {
    if (event) {
      this.filter.pageIndex = event.pageIndex + 1;
      this.filter.pageSize = event.pageSize;
    }
    this.service.filter(this.filter)
      .subscribe({
        next: (res) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
              this.dataSource = new MatTableDataSource<GenActionItemDto>(this.data);
            }
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        },
        complete: () => {
          this.isLoading = false;
        }
      });
  }

  jumpTo(pageNumber: string): void {
    const number = parseInt(pageNumber);
    if (number > 0 && number < this.paginator.getNumberOfPages()) {
      this.filter.pageIndex = number;
      this.getList();
    }
  }

  initForm(): void {
    this.addForm = new FormGroup({
      name: new FormControl(null, [Validators.required, Validators.maxLength(40)]),
      description: new FormControl(null, [Validators.maxLength(200)])
    });

  }
  openAddDialog(item: GenActionItemDto | null = null, isEditable = false): void {
    this.initForm();
    this.isEditable = isEditable;
    if (this.isEditable && item) {
      this.currentItem = item;
      this.name?.setValue(item?.name);
      this.description?.setValue(item?.description);
    }
    this.dialogRef = this.dialog.open(this.addTmpl, {
      minWidth: '400px',
      maxHeight: '98vh'
    })
    this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }

  save(): void {
    if (this.addForm.invalid) {
      this.snb.open('请检查输入项');
      return;
    }
    if (this.isEditable) {
      const data = this.addForm.value as GenActionUpdateDto;
      this.service.update(this.currentItem.id, data)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('更新成功');
              this.dialogRef.close(true);
            } else {
              this.snb.open('更新失败');
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complete: () => {
          }
        });
    } else {
      this.addDto = this.addForm.value as GenActionAddDto;
      this.service.add(this.addDto)
        .subscribe({
          next: (res) => {
            if (res) {
              this.snb.open('添加成功');
              this.dialogRef.close(true);
            } else {
              this.snb.open('添加失败');
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

  deleteConfirm(item: GenActionItemDto): void {
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
  delete(item: GenActionItemDto): void {
    this.isProcessing = true;
    this.service.delete(item.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = this.data.filter(_ => _.id !== item.id);
            this.dataSource.data = this.data;
            this.snb.open('删除成功');
          } else {
            this.snb.open('删除失败');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
        },
        complete: () => {
          this.isProcessing = false;
        }
      });
  }

  /**
   * 编辑
   */
  edit(id: string): void {
    this.router.navigate(['../edit/' + id], { relativeTo: this.route });
  }
}
