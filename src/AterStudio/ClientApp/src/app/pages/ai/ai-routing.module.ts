import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IndexComponent } from './index/index.component';
import { ToolsComponent } from './tools/tools.component';

const routes: Routes = [
  {
    path: 'ai',
    children: [

      { path: 'index', component: IndexComponent },
      { path: 'tools', component: ToolsComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AiRoutingModule { }
