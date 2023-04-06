import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DocsComponent } from './docs/docs.component';
import { DtoComponent } from './dto/dto.component';
import { IndexComponent } from './index/index.component';
import { NavigationComponent } from './navigation/navigation.component';

const routes: Routes = [
  {
    path: 'workspace',
    component: NavigationComponent,
    children: [
      { path: 'code', component: IndexComponent },
      { path: 'docs', component: DocsComponent },
      { path: 'code/dto/:name', component: DtoComponent }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class WorkspaceRoutingModule { }
