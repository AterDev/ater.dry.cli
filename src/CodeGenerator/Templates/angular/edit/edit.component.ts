import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { {$EntityName} } from 'src/app/services/admin/{$EntityPathName}/models/{$EntityPathName}.model';
import { {$EntityName}Service } from 'src/app/services/admin/{$EntityPathName}/{$EntityPathName}.service';
import { {$EntityName}UpdateDto } from 'src/app/services/admin/{$EntityPathName}/models/{$EntityPathName}-update-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Location } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

[@Imports]
@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.css']
})
export class EditComponent implements OnInit {
  [@Declares]
  id!: string;
  isLoading = true;
  isProcessing = false;
  data = {} as {$EntityName};
  updateData = {} as {$EntityName}UpdateDto;
  formGroup!: FormGroup;
    constructor(
    [@DI]
    private service: {$EntityName}Service,
    private snb: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    public dialogRef: MatDialogRef<EditComponent>,
    @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {

    
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      this.id = dlgData.id;
    }
  }

{$DefinedProperties}

  ngOnInit(): void {
    this.getDetail();
    [@Init]
    // TODO:等待数据加载完成
    // this.isLoading = false;
  }
  [@Methods]
  getDetail(): void {
    this.service.getDetail(this.id)
      .subscribe({
        next: (res) => {
          if (res) {
            this.data = res;
            this.initForm();
            this.isLoading = false;
          } else {
            this.snb.open('');
          }
        },
        error: (error) => {
          this.snb.open(error.detail);
          this.isLoading = false;
        }
      });
  }

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
  edit(): void {
    if(this.formGroup.valid) {
      this.isProcessing = true;
      this.updateData = this.formGroup.value as {$EntityName}UpdateDto;
      this.service.update(this.id, this.updateData)
        .subscribe({
          next: (res) => {
            if(res){
              this.snb.open('修改成功');
              this.dialogRef.close(res);
              // this.router.navigate(['../../index'], { relativeTo: this.route });
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
