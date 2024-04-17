import { NgModule } from '@angular/core';
import { WorkspaceRoutingModule } from './workspace-routing.module';
import { IndexComponent } from './index/index.component';
import { ComponentsModule } from 'src/app/components/components.module';
import { ShareModule } from 'src/app/share/share.module';
import { NavigationComponent } from './navigation/navigation.component';
import { DocsComponent } from './docs/docs.component';
import { DtoComponent } from './dto/dto.component';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { MarkdownModule } from 'ngx-markdown';
import { SettingComponent } from './setting/setting.component';
import { DatabaseComponent } from './database/database.component';
import { EntityComponent } from './entity/entity.component';
import { FeatureComponent } from './feature/feature.component';

@NgModule({
  declarations: [
    IndexComponent,
    NavigationComponent,
    DocsComponent,
    DtoComponent,
    SettingComponent,
    DatabaseComponent,
    EntityComponent,
    FeatureComponent,
  ],
  imports: [
    ComponentsModule,
    ShareModule,
    WorkspaceRoutingModule,
    MonacoEditorModule,
    MarkdownModule.forRoot()
  ]
})
export class WorkspaceModule { }
