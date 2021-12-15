<app-layout></app-layout>
<mat-drawer-container autosize fxLayout="row" style="min-height: 800px">
  <mat-drawer mode="side" opened>
    <mat-nav-list fxShow>
      <mat-list-item [routerLink]="['./index']" routerLinkActive="active">
        <mat-icon mat-list-icon>list</mat-icon>
        <a matLine>列表</a>
      </mat-list-item>
    </mat-nav-list>
  </mat-drawer>
  <mat-drawer-content style="padding:8px 16px;width:100%">
    <router-outlet></router-outlet>
  </mat-drawer-content>
</mat-drawer-container>
