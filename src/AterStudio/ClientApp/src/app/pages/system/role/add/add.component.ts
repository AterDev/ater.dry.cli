import { Component, Inject, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { SystemRoleService } from 'src/app/share/services/system-role.service';
import { SystemRole } from 'src/app/share/models/system-role/system-role.model';
import { SystemRoleAddDto } from 'src/app/share/models/system-role/system-role-add-dto.model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Location } from '@angular/common';

@Component({
    selector: 'app-add',
    templateUrl: './add.component.html',
    styleUrls: ['./add.component.css']
})
export class AddComponent implements OnInit {
    
    formGroup!: FormGroup;
    data = {} as SystemRoleAddDto;
    isLoading = true;
    constructor(
        
        private service: SystemRoleService,
        public snb: MatSnackBar,
        private router: Router,
        private route: ActivatedRoute,
        private location: Location
        // public dialogRef: MatDialogRef<AddComponent>,
        // @Inject(MAT_DIALOG_DATA) public dlgData: { id: '' }
    ) {

    }

    get name() { return this.formGroup.get('name'); }
    get nameValue() { return this.formGroup.get('nameValue'); }
    get isSystem() { return this.formGroup.get('isSystem'); }
    get icon() { return this.formGroup.get('icon'); }


  ngOnInit(): void {
    this.initForm();
    
    // TODO:获取其他相关数据后设置加载状态
    this.isLoading = false;
  }
  
  initForm(): void {
    this.formGroup = new FormGroup({
      name: new FormControl(null, [Validators.maxLength(30)]),
      nameValue: new FormControl(null, []),
      isSystem: new FormControl(null, []),
      icon: new FormControl(null, [Validators.maxLength(30)]),

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

  add(): void {
    if(this.formGroup.valid) {
    const data = this.formGroup.value as SystemRoleAddDto;
    this.data = { ...data, ...this.data };
    this.service.add(this.data)
        .subscribe(res => {
            this.snb.open('添加成功');
            // this.dialogRef.close(res);
            this.router.navigate(['../index'],{relativeTo: this.route});
        });
    }
  }
  back(): void {
    this.location.back();
  }
}
