<div class="d-flex gap-1">
  <mat-toolbar>
    <mat-toolbar-row style="font-size:16px">
      <div class="d-flex">
        <!-- 筛选 -->
      </div>
      <div class="d-flex flex-grow-1"></div>
      
    </mat-toolbar-row>
  </mat-toolbar>
</div>
<!-- 加载框 -->
<div class="d-flex text-center justify-content-center mt-2">
  <mat-spinner mode="indeterminate" *ngIf="isLoading">
  </mat-spinner>
</div>

<div *ngIf="!isLoading">
  <!-- 无数据时显示 -->
  <ng-container *ngIf="data && data.length<=0; else elseTemplate">
    <h4>
      暂无内容！
    </h4>
  </ng-container>
  <ng-template #elseTemplate>
    <table mat-table [dataSource]="dataSource" style="width: 100%;">
    {$ColumnsDef}
      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef>操作</th>
        <td mat-cell *matCellDef="let element">
          <button mat-icon-button color="link" [routerLink]="['../detail',element.id]" matTooltip="查看">
            <mat-icon>pages</mat-icon>
          </button>
          <button mat-icon-button color="primary" (click)="edit(element.id)" matTooltip="编辑">
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
    <mat-paginator [pageSizeOptions]="pageSizeOption" [pageIndex]="filter.pageIndex!-1" [pageSize]="filter.pageSize"
      [length]="total" (page)="getList($event)" showFirstLastButtons></mat-paginator>
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