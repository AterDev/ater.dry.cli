import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { {$ServiceName}Service } from 'src/app/share/admin/services/{$ServicePathName}.service';
import { {$ModelName} } from 'src/app/share/admin/models/{$ModelPathName}/{$ModelPathName}.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
[@Imports]
@Component({
    selector: 'app-add',
    templateUrl: './add.component.html',
    styleUrls: ['./add.component.css']
})
export class AddComponent implements OnInit {
[@Declares]
  formGroup!: FormGroup;
  data = {} as {$ModelName};
  isLoading = true;
  isProcessing = false;
  constructor(
      [@DI]
      private service: {$ServiceName}Service,
      public snb: MatSnackBar,
      private router: Router,
      private route: ActivatedRoute,

  ) {

  }

{$DefinedProperties}

  ngOnInit(): void {
    this.initForm();
    [@Init]
  }
  [@Methods]
  initForm(): void {
    this.formGroup = new FormGroup({
{$DefinedFormControls}
    });
  }
  getValidatorMessage(type: string): string {
    switch (type) {
{$DefinedValidatorMessage}
      default:
    return '';
    }
  }

  add(): void {
    if (this.formGroup.valid) {
    this.isProcessing = true;
    const data = this.formGroup.value as {$ModelName};
    this.service.add(data)
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('添加成功');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isProcessing = false;
        },
        complete: () => {
          this.isProcessing = false;
        }
      });
    } else {
        this.snb.open('表单验证不通过，请检查填写的内容!');
    }
  }
}
