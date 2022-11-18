import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { WorkspaceRoutingModule } from './workspace-routing.module';
import { IndexComponent } from './index/index.component';
import { ComponentsModule } from 'src/app/components/components.module';
import { ShareModule } from 'src/app/share/share.module';


@NgModule({
  declarations: [
    IndexComponent
  ],
  imports: [
    ComponentsModule,
    ShareModule,
    WorkspaceRoutingModule
  ]
})
export class WorkspaceModule { }
