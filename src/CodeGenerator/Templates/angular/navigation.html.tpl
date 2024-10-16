﻿<mat-sidenav-container class="example-container">
  <mat-sidenav #sidenav mode="side" opened="true">
    <div class="d-flex flex-row-reverse">
      <button mat-icon-button *ngIf="opened" (click)="toggle()">
        <mat-icon>keyboard_double_arrow_left</mat-icon>
      </button>
      <button mat-icon-button *ngIf="!opened" (click)="toggle()">
        <mat-icon>keyboard_double_arrow_right</mat-icon>
      </button>
    </div>
    <mat-accordion class="example-headers-align" multi>
      <!-- 系统配置 -->
      <mat-expansion-panel hideToggle>
        <mat-expansion-panel-header>
          <mat-panel-title>
            <mat-icon>settings_suggest</mat-icon>
            <span *ngIf="opened">系统配置</span>
          </mat-panel-title>
        </mat-expansion-panel-header>
        <mat-nav-list>
          <a mat-list-item routerLink="/system/setting" routerLinkActive="active">
            <mat-icon>setting</mat-icon>
            <span *ngIf="opened">网站设置</span>
          </a>
        </mat-nav-list>
      </mat-expansion-panel>
      <!-- 根据模型同步生成的菜单 -->
      <ng-container *ngTemplateOutlet="automenu">
      </ng-container>
    </mat-accordion>
  </mat-sidenav>
  <mat-sidenav-content class="px-2">
    <router-outlet></router-outlet>
  </mat-sidenav-content>
</mat-sidenav-container>
<!-- 自动生成菜单内容 -->
<ng-template #automenu>
#@Menus#
</ng-template>