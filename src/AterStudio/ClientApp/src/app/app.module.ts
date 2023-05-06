import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ComponentsModule } from './components/components.module';
import { AppRoutingModule } from './app-routing.module';
import { HomeModule } from './pages/home/home.module';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { MAT_SNACK_BAR_DEFAULT_OPTIONS } from '@angular/material/snack-bar';
import { CustomerHttpInterceptor } from './share/customer-http.interceptor';
import { WorkspaceModule } from './pages/workspace/workspace.module';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { MarkdownModule, MarkedOptions, ClipboardOptions, ClipboardButtonComponent, MarkedRenderer } from 'ngx-markdown';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    ComponentsModule,
    HomeModule,
    WorkspaceModule,
    MarkdownModule.forRoot({
      markedOptions: {
        provide: MarkedOptions,
        useFactory: markedOptionsFactory
      },
      clipboardOptions: {
        provide: ClipboardOptions,
        useValue: {
          buttonComponent: ClipboardButtonComponent,
        },
      },
    }),
    MonacoEditorModule.forRoot()
  ],
  providers: [
    { provide: MAT_SNACK_BAR_DEFAULT_OPTIONS, useValue: { duration: 2500 } },
    { provide: HTTP_INTERCEPTORS, useClass: CustomerHttpInterceptor, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function markedOptionsFactory(): MarkedOptions {
  const renderer = new MarkedRenderer();
  renderer.blockquote = (text: string) => {
    return '<blockquote class="blockquote"><p>' + text + '</p></blockquote>';
  };
  // renderer.code = (code: string) => {
  //   return '<code class="inline-code">' + code + '</code>'
  // }
  return {
    renderer: renderer,
    gfm: true,
    breaks: false,
    pedantic: false,
    smartLists: true,
    smartypants: false,
  };
}
