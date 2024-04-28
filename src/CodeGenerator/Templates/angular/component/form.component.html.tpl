<h1 class="mt-1">创建表单</h1>
<form class="d-flex flex-column" *ngIf="!isLoading" [formGroup]="formGroup">
  <div class="d-flex flex-column">
#@FormControls#
    <div class="d-flex mt-2">
      <button mat-flat-button color="primary" (click)="add()">保存</button>
    </div>
  </div>
</form>

