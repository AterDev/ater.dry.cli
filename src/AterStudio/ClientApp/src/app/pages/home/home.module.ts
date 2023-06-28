import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { HomeRoutingModule } from './home-routing.module';
import { LoginComponent } from './login/login.component';
import { IndexComponent } from './index/index.component';
import { ShareModule } from 'src/app/share/share.module';
import { CreateComponent } from './create/create.component';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { UihelperComponent } from './uihelper/uihelper.component';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';


@NgModule({
  declarations: [
    LoginComponent,
    IndexComponent,
    CreateComponent,
    UihelperComponent,
  ],
  imports: [
    ShareModule,
    HomeRoutingModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MonacoEditorModule
  ]
})
export class HomeModule { }
