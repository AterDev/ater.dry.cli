import { NgModule } from '@angular/core';
import { RouteReuseStrategy, RouterModule, Routes } from '@angular/router';
import { DocsComponent } from './docs/docs.component';
import { DtoComponent } from './dto/dto.component';
import { IndexComponent } from './index/index.component';
import { NavigationComponent } from './navigation/navigation.component';
import { CustomRouteReuseStrategy } from 'src/app/custom-route-strategy';
import { SettingComponent } from './setting/setting.component';

const routes: Routes = [
  {
    path: 'workspace',
    component: NavigationComponent,
    children: [
      { path: 'code', component: IndexComponent },
      { path: 'docs', component: DocsComponent },
      { path: 'setting', component: SettingComponent },
      { path: 'code/dto/:name', component: DtoComponent }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
  providers: [{
    provide: RouteReuseStrategy,
    useClass: CustomRouteReuseStrategy
  }]
})
export class WorkspaceRoutingModule { }
