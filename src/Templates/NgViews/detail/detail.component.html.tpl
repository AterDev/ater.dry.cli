<mat-toolbar fxLayoutGap="4px">
  <button color="basic" mat-icon-button matTooltip="返回" (click)="back()">
    <mat-icon>arrow_back</mat-icon>
  </button>
  <button color="primary" mat-icon-button matTooltip="编辑" (click)="edit()">
    <mat-icon>edit</mat-icon>
  </button>
</mat-toolbar>
<div *ngIf="!isLoading" fxLayout="row" fxLayoutAlign="start start" fxLayoutGap="8px">
  <mat-card class="" style="width: 100%;">
    <!-- <mat-card-header>
      <div mat-card-avatar class=""></div>
      <mat-card-title></mat-card-title>
      <mat-card-subtitle></mat-card-subtitle>
    </mat-card-header> -->
    <!-- <img mat-card-image src="" alt=""> -->
    <mat-card-content>
{$Content}
    </mat-card-content>
    <mat-card-actions>
      <!-- <button mat-button></button>
      <button mat-button></button> -->
    </mat-card-actions>
  </mat-card>
</div>
