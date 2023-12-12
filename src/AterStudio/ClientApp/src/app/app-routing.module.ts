import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { IndexComponent } from './pages/tools/index/index.component';

const routes: Routes = [
    { path: '', redirectTo: 'index', pathMatch: 'full' },
    { path: 'index', redirectTo: 'index', pathMatch: 'full' },
    { path: 'login', redirectTo: 'login', pathMatch: 'full' },
    { path: 'ai', redirectTo: 'ai/index', pathMatch: 'full' },
    { path: 'tools', redirectTo: 'tools/index', pathMatch: 'full' },
    { path: 'tools/index', component: IndexComponent },
    { path: '*', redirectTo: '', pathMatch: 'full' },

];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }