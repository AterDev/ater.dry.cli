import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { {$EntityName}Service } from 'src/app/share/admin/services/{$EntityPathName}.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { {$EntityName}ItemDto } from 'src/app/share/admin/models/{$EntityPathName}/{$EntityPathName}-item-dto.model';
import { {$EntityName}FilterDto } from 'src/app/share/admin/models/{$EntityPathName}/{$EntityPathName}-filter-dto.model';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { AddComponent } from '../add/add.component';
import { EditComponent } from '../edit/edit.component';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.css']
})
export class IndexComponent implements OnInit {
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: {$EntityName}ItemDto[] = [];
  columns: string[] = [{$Columns}];
  dataSource!: MatTableDataSource<{$EntityName}ItemDto>;
  dialogRef!: MatDialogRef<{}, any>;
  @ViewChild('myDialog', { static: true })
  myTmpl!: TemplateRef<{}>;
  mydialogForm!: FormGroup;
  filter: {$EntityName}FilterDto;
  pageSizeOption = [12, 20, 50];
  constructor(
    private service: {$EntityName}Service,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
  ) {

    this.filter = {
      pageIndex: 1,
      pageSize: 12
    };
  }

  ngOnInit(): void {
    this.getList();
  }

  getList(event?: PageEvent): void {
    if(event) {
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
              this.dataSource = new MatTableDataSource<{$EntityName}ItemDto>(this.data);
            }
          } else {
            this.snb.open('');
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

  openAddDialog(): void {
    this.dialogRef = this.dialog.open(AddComponent, {
      minWidth: '400px',
    })
      this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }

  openEditDialog(item: AnnounceItemDto): void {
    this.dialogRef = this.dialog.open(EditComponent, {
      minWidth: '400px',
      data: { id: item.id }
    })
      this.dialogRef.afterClosed()
      .subscribe(res => {
        if (res)
          this.getList();
      });
  }


  deleteConfirm(item: {$EntityName}ItemDto): void {
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
  delete(item: {$EntityName}ItemDto): void {
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
      complete: ()=>{
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
