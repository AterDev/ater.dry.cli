import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnumPipe } from './pipe/enum.pipe';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToKeyValuePipe } from './pipe/to-key-value.pipe';
import { ComponentsModule } from '../components/components.module';
import { EnumTextPipe } from './pipe/enum-text.pipe';

@NgModule({
  declarations: [ToKeyValuePipe, EnumTextPipe],
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    ComponentsModule
  ],
  exports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    ComponentsModule,
    EnumTextPipe,
    ToKeyValuePipe
  ]
})
export class ShareModule { }
