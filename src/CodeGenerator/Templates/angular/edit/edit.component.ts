import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { {$EntityName} } from 'src/app/share/admin/models/{$EntityPathName}/{$EntityPathName}.model';
import { {$EntityName}Service } from 'src/app/share/admin/services/{$EntityPathName}.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { {$EntityName}UpdateDto } from 'src/app/share/admin/models/{$EntityPathName}/{$EntityPathName}-update-dto.model';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Location } from '@angular/common';
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
    private location: Location
    // public dialogRef: MatDialogRef<EditComponent>,
    // @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
  ) {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.id = id;
    } else {
      // TODO: id为空
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
      .subscribe(res => {
        this.data = res;
        this.initForm();
        this.isLoading = false;
      }, error => {
        this.snb.open(error);
      })
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
              // this.dialogRef.close(res);
              this.router.navigate(['../../index'], { relativeTo: this.route });
            }
          },
          error: (error) => {
            this.snb.open(error.detail);
          },
          complate: () => {
            this.isProcessing = false;
          }
        });
    }
  }

  back(): void {
    this.location.back();
  }

}
