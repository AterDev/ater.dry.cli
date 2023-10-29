import { NgModule } from '@angular/core';

import { AiRoutingModule } from './ai-routing.module';
import { IndexComponent } from './index/index.component';
import { ShareModule } from 'src/app/share/share.module';
import { MarkdownModule } from 'ngx-markdown';


@NgModule({
  declarations: [
    IndexComponent,
  ],
  imports: [
    ShareModule,
    MarkdownModule.forRoot(),
    AiRoutingModule
  ]
})
export class AiModule { }
