<mat-toolbar class="d-flex gap-1">
  <button color="basic" mat-icon-button matTooltip="返回" (click)="back()">
    <mat-icon>arrow_back</mat-icon>
  </button>
    编辑
</mat-toolbar>
<form class="d-flex" *ngIf="!isLoading" [formGroup]="formGroup" (ngSubmit)="edit()">
  <div class="d-flex flex-column w-100">
{$FormControls}
    <div class="d-flex mt-1">
      <button class="d-flex flex-column" mat-flat-button color="primary" type="submit">保存</button>
    </div>
  </div>
</form>
