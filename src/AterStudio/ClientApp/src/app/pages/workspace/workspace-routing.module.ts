import { NgModule } from '@angular/core';
import { RouteReuseStrategy, RouterModule, Routes } from '@angular/router';
import { DocsComponent } from './docs/docs.component';
import { DtoComponent } from './dto/dto.component';
import { IndexComponent } from './index/index.component';
import { NavigationComponent } from './navigation/navigation.component';
import { CustomRouteReuseStrategy } from 'src/app/custom-route-strategy';
import { SettingComponent } from './setting/setting.component';
import { DatabaseComponent } from './database/database.component';
import { EntityComponent } from './entity/entity.component';

const routes: Routes = [
  {
    path: 'workspace',
    component: NavigationComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'index' },
      { path: 'index', component: IndexComponent },
      { path: 'entity', component: EntityComponent },
      { path: 'docs', component: DocsComponent },
      { path: 'database', component: DatabaseComponent },
      { path: 'setting', component: SettingComponent },
      { path: 'index/dto/:name', component: DtoComponent }
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
