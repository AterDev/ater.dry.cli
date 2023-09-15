import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AiRoutingModule } from './ai-routing.module';
import { IndexComponent } from './index/index.component';
import { ShareModule } from 'src/app/share/share.module';


@NgModule({
  declarations: [
    IndexComponent
  ],
  imports: [
    ShareModule,
    AiRoutingModule
  ]
})
export class AiModule { }
