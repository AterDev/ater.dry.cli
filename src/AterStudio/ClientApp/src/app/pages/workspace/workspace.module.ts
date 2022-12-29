import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { WorkspaceRoutingModule } from './workspace-routing.module';
import { IndexComponent } from './index/index.component';
import { ComponentsModule } from 'src/app/components/components.module';
import { ShareModule } from 'src/app/share/share.module';
import { NavigationComponent } from './navigation/navigation.component';
import { DocsComponent } from './docs/docs.component';


@NgModule({
  declarations: [
    IndexComponent,
    NavigationComponent,
    DocsComponent
  ],
  imports: [
    ComponentsModule,
    ShareModule,
    WorkspaceRoutingModule
  ]
})
export class WorkspaceModule { }
