import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButton, MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormField, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { EnumTextPipeModule } from 'src/app/pipe/enum-text.pipe';
import { StringConvertType } from 'src/app/services/enum/models/string-convert-type.model';
import { ToolsService } from 'src/app/services/tools/tools.service';
import { ToKeyValuePipe, ToKeyValuePipeModule } from 'src/app/share/pipe/to-key-value.pipe';

@Component({
  selector: 'app-string',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatFormFieldModule, FormsModule, MatInputModule,
    EnumTextPipeModule, ToKeyValuePipeModule,],
  templateUrl: './string.component.html',
  styleUrl: './string.component.css'
})
export class StringComponent {
  StringConvertType = StringConvertType;
  content: string | null = null;
  result: Map<string, string> | null = null;
  constructor(
    private dialogRef: MatDialogRef<{}, any>,
    private snb: MatSnackBar,
    private service: ToolsService
  ) {
  }

  convertString(type: StringConvertType): void {
    if (type === StringConvertType.Guid) {
      this.content = "guid";
    }
    if (this.content === null) {
      this.snb.open('请输入字符串！');
      return;
    }

    this.service.convertString(this.content, type)
      .subscribe({
        next: (res) => {
          if (res) {
            console.log(res);
            this.result = new Map<string, string>(Object.entries(res));
          }
        },
        error: (error) => {
        },
        complete: () => {
        }
      });
  }
}
