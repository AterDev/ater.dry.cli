<mat-toolbar fxLayoutGap="4px">
  <button color="basic" mat-icon-button matTooltip="返回" (click)="back()">
    <mat-icon>arrow_back</mat-icon>
  </button>
    编辑
</mat-toolbar>
<form *ngIf=""!isLoading"" [formGroup]=""formGroup"" (ngSubmit)=""edit()"">
  <div fxLayout=""row wrap"" fxLayoutAlign=""start start"" fxLayoutGap=""8px"">
{$FormControls}
  </div>
  <div fxLayout=""row"" fxLayoutAlign=""start start"" fxLayoutGap=""8px"" class=""mt-2"">
    <button mat-flat-button color=""primary"" type=""submit"">保存</button>
  </div>
</form>
