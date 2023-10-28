import { NgModule } from '@angular/core';

import { AiRoutingModule } from './ai-routing.module';
import { IndexComponent } from './index/index.component';
import { ShareModule } from 'src/app/share/share.module';
import { MarkdownModule } from 'ngx-markdown';
import { ToolsComponent } from './tools/tools.component';


@NgModule({
  declarations: [
    IndexComponent,
    ToolsComponent
  ],
  imports: [
    ShareModule,
    MarkdownModule.forRoot(),
    AiRoutingModule
  ]
})
export class AiModule { }
