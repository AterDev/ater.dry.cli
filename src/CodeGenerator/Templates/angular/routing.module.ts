import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { AuthGuard } from 'src/app/auth.guard';
import { IndexComponent } from './index/index.component';
import { AddComponent } from './add/add.component';
import { DetailComponent } from './detail/detail.component';
import { EditComponent } from './edit/edit.component';

const routes: Routes = [
  {
    path: '{$ModulePathName}',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children:
      [
        {
          path: '',
          canActivateChild: [AuthGuard],
          children: [
            { path: '', pathMatch: 'full', redirectTo: 'index' },
            { path: 'index', component: IndexComponent },
            { path: 'add', component: AddComponent },
            { path: 'detail/:id', component: DetailComponent },
            { path: 'edit/:id', component: EditComponent },
          ]
        }
      ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class {$ModuleName}RoutingModule { }
