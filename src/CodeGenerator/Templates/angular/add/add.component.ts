import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { {$EntityName}Service } from 'src/app/share/services/{$EntityPathName}.service';
import { {$EntityName} } from 'src/app/share/models/{$EntityPathName}/{$EntityPathName}.model';
import { {$EntityName}AddDto } from 'src/app/share/models/{$EntityPathName}/{$EntityPathName}-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Location } from '@angular/common';
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
    constructor(
        [@DI]
        private service: {$EntityName}Service,
        public snb: MatSnackBar,
        private router: Router,
        private route: ActivatedRoute,
        private location: Location
        // public dialogRef: MatDialogRef<AddComponent>,
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
    const data = this.formGroup.value as {$EntityName}AddDto;
    this.service.add(data)
      .subscribe({
        next: (res) => {
          if (res) {
            this.snb.open('添加成功');
            // this.dialogRef.close(res);
            this.router.navigate(['../index'], { relativeTo: this.route });
          }

        },
        error: () => {

        }
      });
    }
  }
  back(): void {
    this.location.back();
  }
}
