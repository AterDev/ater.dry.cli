import { NgModule } from '@angular/core';
import { LayoutComponent } from '../components/layout/layout.component';
// import { NavigationComponent } from './navigation/navigation.component';
import { ShareModule } from '../share/share.module';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
// import { CKEditorModule } from '@ckeditor/ckeditor5-angular';

@NgModule({
  declarations: [
    LayoutComponent,
   // NavigationComponent,
    ConfirmDialogComponent,
  
  ],
  imports: [
    ShareModule,
    //CKEditorModule
  ],
  exports: [
    LayoutComponent,
    ConfirmDialogComponent,
    // CKEditorModule
  ]
})
export class ComponentsModule { }
