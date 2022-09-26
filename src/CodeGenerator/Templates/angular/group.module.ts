import { NgModule } from '@angular/core';
import { {$ModuleName}RoutingModule } from './{$ModulePathName}-routing.module';
import { ShareModule } from 'src/app/share/share.module';
import { ComponentsModule } from 'src/app/components/components.module';
{$ImportModulesPath}

@NgModule({
  declarations: [],
  imports: [
    ComponentsModule,
    ShareModule,
    {$ModuleName}RoutingModule,
    {$ImportModules}

  ]
})
export class {$ModuleName}Module { }
