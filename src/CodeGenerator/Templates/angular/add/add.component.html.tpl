<mat-toolbar fxLayoutGap="4px">
  <button color="basic" mat-icon-button matTooltip="返回" (click)="back()">
    <mat-icon>arrow_back</mat-icon>
  </button>
    添加
</mat-toolbar>
<form class="d-flex" *ngIf="!isLoading" [formGroup]="formGroup" (ngSubmit)="add()">
  <div class="d-flex flex-column w-100">
{$FormControls}
    <div class="d-flex">
      <button class="d-flex flex-column" mat-flat-button color="primary" type="submit">保存</button>
    </div>
  </div>
</form>

