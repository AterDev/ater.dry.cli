
<div class="d-flex flex-column" style="height: 100vh;">
<div class="top-bar">
  <mat-toolbar>
    <mat-toolbar-row style="font-size:16px" class="d-flex gap-1">
      <button mat-icon-button color="primary" matTooltip="筛选" (click)="toggleFilter()">
        <mat-icon fontSet="material-icons-outlined">filter_alt</mat-icon>
      </button>
      <div class="d-flex flex-grow-1"></div>
      <button mat-icon-button color="primary" matTooltip="添加" routerLink="../add">
        <mat-icon>add</mat-icon>
      </button>
    </mat-toolbar-row>
  </mat-toolbar>
</div>

@if(isLoading){
<!-- 加载框 -->
<div class="d-flex text-center justify-content-center mt-2">
  <mat-spinner mode="indeterminate">
  </mat-spinner>
</div>
}@else {}

<div class="list-container">
  <!-- 无数据时显示 -->
    @if(data && data.length<=0){ 
      <h4>暂无数据！</h4>
    }@else {
    <mat-nav-list>
      @for (element of data; track $index) {
      <mat-list-item (click)="toDetail(element.id)">
//[@ColumnsDef]        
      </mat-list-item>
      }
    </mat-nav-list>
    }
</div>
</div>


<!-- 筛选控件 -->
<ng-template #filterSheet>
  <div class="d-flex justify-content-between align-items-center">
    <h4>筛选器</h4>
    <button mat-icon-button color="primary" matTooltip="" (click)="clearSearch()">
      <mat-icon fontSet="material-icons-outlined">filter_alt_off</mat-icon>
    </button>
  </div>
  <div class="d-flex gap-1 flex-column">
//[@FilterForm]
    <button mat-raised-button color="primary" (click)="search()">提交筛选</button>
  </div>
</ng-template>