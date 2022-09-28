import { NgModule } from '@angular/core';
import { {$ModuleName}RoutingModule } from './{$ModulePathName}-routing.module';
import { ShareModule } from 'src/app/share/share.module';
import { ComponentsModule } from 'src/app/components/components.module';
import { RouteReuseStrategy } from '@angular/router';
import { CustomRouteReuseStrategy } from 'src/app/custom-route-strategy';
{$ImportModulesPath}

@NgModule({
  declarations: [],
  imports: [
    ComponentsModule,
    ShareModule,
    {$ModuleName}RoutingModule,
    {$ImportModules}
  ],
  providers: [{
    provide: RouteReuseStrategy,
    useClass: CustomRouteReuseStrategy
  }]
})
export class {$ModuleName}Module { }
