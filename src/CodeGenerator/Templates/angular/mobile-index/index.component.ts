import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmDialogComponent } from 'src/app/components/confirm-dialog/confirm-dialog.component';
import { #@EntityName#Service } from 'src/app/services/admin/#@EntityPathName#/#@EntityPathName#.service';
import { #@EntityName#ItemDto } from 'src/app/services/admin/#@EntityPathName#/models/#@EntityPathName#-item-dto.model';
import { #@EntityName#FilterDto } from 'src/app/services/admin/#@EntityPathName#/models/#@EntityPathName#-filter-dto.model';
import { #@EntityName#ItemDtoPageList } from 'src/app/services/admin/#@EntityPathName#/models/#@EntityPathName#-item-dto-page-list.model';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatBottomSheet, MatBottomSheetRef } from '@angular/material/bottom-sheet';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { AddComponent } from '../add/add.component';
import { EditComponent } from '../edit/edit.component';
import { Observable, forkJoin } from 'rxjs';

@Component({
  selector: 'app-index',
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.scss']
})
export class IndexComponent implements OnInit {
  @ViewChild(MatPaginator, { static: true }) paginator!: MatPaginator;
  isLoading = true;
  isProcessing = false;
  total = 0;
  data: #@EntityName#ItemDto[] =[];
  @ViewChild("filterSheet", { static: true }) filterSheet!: TemplateRef<{}>;
  bottomSheetRef!: MatBottomSheetRef<{}>;
  filter: #@EntityName#FilterDto;

  constructor(
    private service: #@EntityName#Service,
    private snb: MatSnackBar,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private _bottomSheet: MatBottomSheet
  ) {

    this.filter = {
      pageIndex: 1,
      pageSize: 12
    };
  }

  ngOnInit(): void {
    forkJoin([this.getListAsync()])
      .subscribe({
        next: ([res]) => {
          if (res) {
            if (res.data) {
              this.data = res.data;
              this.total = res.count;
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

  getListAsync(): Observable<#@EntityName#ItemDtoPageList> {
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
  toggleFilter(): void {
    this.bottomSheetRef = this._bottomSheet.open(this.filterSheet, {});
  }

  jumpTo(pageNumber: string): void {
    const number = parseInt(pageNumber);
    if (number > 0 && number < this.paginator.getNumberOfPages()) {
      this.filter.pageIndex = number;
      this.getList();
    }
  }
  search(): void {
    this.bottomSheetRef.dismiss();
    this.getList();
  }

  clearSearch(): void {
    this.filter = {
      pageIndex: 1,
      pageSize: 12
    };
  }
 
  toDetail(id: string): void {
    this.router.navigate([`../detail/${id}`], { relativeTo: this.route });
  }

}
