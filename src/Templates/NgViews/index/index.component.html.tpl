<div fxLayout="row" fxLayoutAlign="start center" fxLayoutGap="8px">
  <mat-toolbar color="">
    <mat-toolbar-row style="font-size:16px">
      <div fxFlex></div>
      <button mat-flat-button color="primary" matTooltipPosition="right" [routerLink]="['../add']">
        <mat-icon>add</mat-icon>
        添加
      </button>
    </mat-toolbar-row>
  </mat-toolbar>
</div>
<table mat-table [dataSource]="dataSource" style="width: 100%;">
{$ColumnsDef}
  <ng-container matColumnDef="actions">
    <th mat-header-cell *matHeaderCellDef>操作</th>
    <td mat-cell *matCellDef="let element">
      <button mat-icon-button class="text-blue" [routerLink]="['../detail',element.id]" matTooltip="查看">
        <mat-icon>pages</mat-icon>
      </button>
      <button mat-icon-button class="text-blue" (click)="edit(element.id)" matTooltip="编辑">
        <mat-icon>edit</mat-icon>
      </button>
      <button mat-icon-button class="text-accent" matTooltip="删除">
        <mat-icon (click)="deleteConfirm(element)">delete_forever</mat-icon>
      </button>
    </td>
  </ng-container>

  <tr mat-header-row *matHeaderRowDef="columns"></tr>
  <tr mat-row *matRowDef="let row; columns: columns;"></tr>
</table>
<mat-paginator [pageSizeOptions]="pageSizeOption"
[pageIndex]="filter.pageIndex-1" [pageSize]="filter.pageSize"
  [length]="total" (page)="getList($event)"showFirstLastButtons></mat-paginator>
