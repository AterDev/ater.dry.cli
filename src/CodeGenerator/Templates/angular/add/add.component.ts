import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { {$EntityName}Service } from 'src/app/share/admin/services/{$EntityPathName}.service';
import { {$EntityName} } from 'src/app/share/admin/models/{$EntityPathName}/{$EntityPathName}.model';
import { {$EntityName}AddDto } from 'src/app/share/admin/models/{$EntityPathName}/{$EntityPathName}-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { MatDialogRef } from '@angular/material/dialog';

[@Imports]
@Component({
    selector: 'app-add',
    templateUrl: './add.component.html',
    styleUrls: ['./add.component.css']
})
export class AddComponent implements OnInit {
    [@Declares]
    formGroup!: FormGroup;
    data = {} as {$EntityName}AddDto;
    isLoading = true;
    isProcessing = false;
    constructor(
        [@DI]
        private service: {$EntityName}Service,
        public snb: MatSnackBar,
        private router: Router,
        private route: ActivatedRoute,
        private location: Location,
         public dialogRef: MatDialogRef<AddComponent>,
        // @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
    ) {

    }

{$DefinedProperties}

  ngOnInit(): void {
    this.initForm();
    [@Init]
    // TODO:获取其他相关数据后设置加载状态
    this.isLoading = false;
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
    if(this.formGroup.valid) {
    this.isProcessing = true;
    const data = this.formGroup.value as {$EntityName}AddDto;
    this.service.add(data)
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('添加成功');
             this.dialogRef.close(res);
            //this.router.navigate(['../index'], { relativeTo: this.route });
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

  back(): void {
    this.location.back();
  }
}
