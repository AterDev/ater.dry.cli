<div class="d-flex">
  <mat-toolbar class="box-shadow">
    <mat-toolbar-row style="font-size:16px">
      <div class="d-flex gap-1">
        <!-- 筛选 -->
        <!--<mat-form-field subscriptSizing="dynamic">
          <mat-label>名称</mat-label>
          <input matInput placeholder="名称" [(ngModel)]="filter.name" (keyup.enter)="getList()">
        </mat-form-field> -->
        <!-- <mat-form-field subscriptSizing="dynamic">
          <mat-label>筛选示例</mat-label>
          <mat-select placeholder="筛选示例" [(ngModel)]="filter.language" (selectionChange)="getList()">
            <mat-option [value]="null">全部</mat-option>
            <mat-option *ngFor="let item of DictLanguage | toKeyValue" [value]="item.value">
              {{item.value | enumText:'DictLanguage'}}
            </mat-option>
          </mat-select>
        </mat-form-field> -->
      </div>
      <div class="d-flex flex-grow-1"></div>
      <button mat-flat-button color="primary" (click)="openAddDialog()">
        <mat-icon>add</mat-icon>
        添加
      </button>
    </mat-toolbar-row>
  </mat-toolbar>
</div>
<!-- 加载框 -->
<div class="d-flex text-center justify-content-center mt-2">
  <mat-spinner mode="indeterminate" *ngIf="isLoading">
  </mat-spinner>
</div>


<div *ngIf="!isLoading" class="px-2">
  <!-- 无数据时显示 -->
  <ng-container *ngIf="data && data.length<=0; else elseTemplate">
    <h4>
      暂无数据！
    </h4>
  </ng-container>
  <ng-template #elseTemplate>
    <table mat-table [dataSource]="dataSource" style="width: 100%;" #table="matTable">
    {$ColumnsDef}
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>操作</th>
        <td mat-cell *matCellDef="let element;table:table">
          <button mat-icon-button color="link" [routerLink]="['../detail',element.id]" matTooltip="查看">
            <mat-icon>pages</mat-icon>
          </button>
          <button mat-icon-button color="primary" (click)="openEditDialog(element)" matTooltip="编辑">
            <mat-icon>edit</mat-icon>
          </button>
          <button mat-icon-button color="warn" matTooltip="删除" (click)="deleteConfirm(element)">
            <mat-icon>delete_forever</mat-icon>
          </button>
        </td>
      </ng-container>
      <tr mat-header-row *matHeaderRowDef="columns"></tr>
      <tr mat-row *matRowDef="let row; columns: columns;"></tr>
    </table>
    <mat-divider></mat-divider>
    <div class="d-flex justify-content-between paginator">
      <mat-form-field subscriptSizing="dynamic">
        <mat-label>跳转到</mat-label>
        <input matInput type="number" [value]="filter.pageIndex" #pageJump (keyup.enter)="jumpTo(pageJump.value)">
      </mat-form-field>
      <mat-paginator [pageSizeOptions]="pageSizeOption" [pageIndex]="filter.pageIndex!-1" [pageSize]="filter.pageSize"
        [length]="total" (page)="getList($event)" showFirstLastButtons>
      </mat-paginator>
    </div>
  </ng-template>
</div>

<ng-template #myDialog>
  <h2 mat-dialog-title>标题</h2>
  <mat-dialog-content>
    <form [formGroup]="mydialogForm">
      <mat-form-field appearance="fill">
        <mat-label>名称</mat-label>
        <input matInput placeholder="名称" formControlName="name" required>
      </mat-form-field>
    </form>
  </mat-dialog-content>
  <mat-dialog-actions class="justify-content-end">
    <button mat-button mat-dialog-close>取消</button>
    <button mat-button color="primary">确认</button>
  </mat-dialog-actions>
</ng-template>