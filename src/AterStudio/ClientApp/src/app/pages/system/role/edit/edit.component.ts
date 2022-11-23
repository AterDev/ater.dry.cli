import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SystemRole } from 'src/app/share/models/system-role/system-role.model';
import { SystemRoleService } from 'src/app/share/services/system-role.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SystemRoleUpdateDto } from 'src/app/share/models/system-role/system-role-update-dto.model';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Location } from '@angular/common';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.css']
})
export class EditComponent implements OnInit {
  
  id!: string;
  isLoading = true;
  data = {} as SystemRole;
  updateData = {} as SystemRoleUpdateDto;
  formGroup!: FormGroup;
    constructor(
    
    private service: SystemRoleService,
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

    get name() { return this.formGroup.get('name'); }
    get nameValue() { return this.formGroup.get('nameValue'); }
    get isSystem() { return this.formGroup.get('isSystem'); }
    get icon() { return this.formGroup.get('icon'); }


  ngOnInit(): void {
    this.getDetail();
    
    // TODO:等待数据加载完成
    // this.isLoading = false;
  }
  
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
      name: new FormControl(this.data.name, [Validators.maxLength(30)]),
      nameValue: new FormControl(this.data.nameValue, []),
      isSystem: new FormControl(this.data.isSystem, []),
      icon: new FormControl(this.data.icon, [Validators.maxLength(30)]),

    });
  }
  getValidatorMessage(type: string): string {
    switch (type) {
      case 'name':
        return this.name?.errors?.['required'] ? 'Name必填' :
          this.name?.errors?.['minlength'] ? 'Name长度最少位' :
            this.name?.errors?.['maxlength'] ? 'Name长度最多30位' : '';
      case 'nameValue':
        return this.nameValue?.errors?.['required'] ? 'NameValue必填' :
          this.nameValue?.errors?.['minlength'] ? 'NameValue长度最少位' :
            this.nameValue?.errors?.['maxlength'] ? 'NameValue长度最多位' : '';
      case 'isSystem':
        return this.isSystem?.errors?.['required'] ? 'IsSystem必填' :
          this.isSystem?.errors?.['minlength'] ? 'IsSystem长度最少位' :
            this.isSystem?.errors?.['maxlength'] ? 'IsSystem长度最多位' : '';
      case 'icon':
        return this.icon?.errors?.['required'] ? 'Icon必填' :
          this.icon?.errors?.['minlength'] ? 'Icon长度最少位' :
            this.icon?.errors?.['maxlength'] ? 'Icon长度最多30位' : '';

      default:
        return '';
    }
  }
  edit(): void {
    if(this.formGroup.valid) {
      this.updateData = this.formGroup.value as SystemRoleUpdateDto;
      this.service.update(this.id, this.updateData)
        .subscribe(res => {
          this.snb.open('修改成功');
           // this.dialogRef.close(res);
          this.router.navigate(['../../index'],{relativeTo: this.route});
        });
    }
  }

  back(): void {
    this.location.back();
  }

}
