<mat-toolbar class="d-flex gap-1">
  <mat-icon matTooltip="返回" (click)="back()">arrow_back</mat-icon>
  <span>编辑</span>
</mat-toolbar>
<form class="d-flex p-2" *ngIf="!isLoading" [formGroup]="formGroup">
  <div class="d-flex flex-column w-100">
{$FormControls}
    <div class="d-flex mt-1">
      <button class="d-flex flex-column" mat-flat-button color="primary" (click)="edit()">保存</button>
    </div>
  </div>
</form>
