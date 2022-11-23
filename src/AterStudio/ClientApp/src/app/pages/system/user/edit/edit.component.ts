import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { SystemUser } from 'src/app/share/models/system-user/system-user.model';
import { SystemUserService } from 'src/app/share/services/system-user.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SystemUserUpdateDto } from 'src/app/share/models/system-user/system-user-update-dto.model';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Location } from '@angular/common';
import { Sex } from 'src/app/share/models/enum/sex.model';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.css']
})
export class EditComponent implements OnInit {
  Sex = Sex;

  id!: string;
  isLoading = true;
  data = {} as SystemUser;
  updateData = {} as SystemUserUpdateDto;
  formGroup!: FormGroup;
    constructor(
    
    private service: SystemUserService,
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

    get userName() { return this.formGroup.get('userName'); }
    get realName() { return this.formGroup.get('realName'); }
    get email() { return this.formGroup.get('email'); }
    get emailConfirmed() { return this.formGroup.get('emailConfirmed'); }
    get phoneNumber() { return this.formGroup.get('phoneNumber'); }
    get phoneNumberConfirmed() { return this.formGroup.get('phoneNumberConfirmed'); }
    get twoFactorEnabled() { return this.formGroup.get('twoFactorEnabled'); }
    get lockoutEnd() { return this.formGroup.get('lockoutEnd'); }
    get lockoutEnabled() { return this.formGroup.get('lockoutEnabled'); }
    get accessFailedCount() { return this.formGroup.get('accessFailedCount'); }
    get lastLoginTime() { return this.formGroup.get('lastLoginTime'); }
    get retryCount() { return this.formGroup.get('retryCount'); }
    get avatar() { return this.formGroup.get('avatar'); }
    get sex() { return this.formGroup.get('sex'); }


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
      userName: new FormControl(this.data.userName, [Validators.maxLength(30)]),
      realName: new FormControl(this.data.realName, [Validators.maxLength(30)]),
      email: new FormControl(this.data.email, [Validators.maxLength(100)]),
      emailConfirmed: new FormControl(this.data.emailConfirmed, []),
      phoneNumber: new FormControl(this.data.phoneNumber, [Validators.maxLength(20)]),
      phoneNumberConfirmed: new FormControl(this.data.phoneNumberConfirmed, []),
      twoFactorEnabled: new FormControl(this.data.twoFactorEnabled, []),
      lockoutEnd: new FormControl(this.data.lockoutEnd, []),
      lockoutEnabled: new FormControl(this.data.lockoutEnabled, []),
      accessFailedCount: new FormControl(this.data.accessFailedCount, []),
      lastLoginTime: new FormControl(this.data.lastLoginTime, []),
      retryCount: new FormControl(this.data.retryCount, []),
      avatar: new FormControl(this.data.avatar, [Validators.maxLength(200)]),
      sex: new FormControl(this.data.sex, []),

    });
  }
  getValidatorMessage(type: string): string {
    switch (type) {
      case 'userName':
        return this.userName?.errors?.['required'] ? 'UserName必填' :
          this.userName?.errors?.['minlength'] ? 'UserName长度最少位' :
            this.userName?.errors?.['maxlength'] ? 'UserName长度最多30位' : '';
      case 'realName':
        return this.realName?.errors?.['required'] ? 'RealName必填' :
          this.realName?.errors?.['minlength'] ? 'RealName长度最少位' :
            this.realName?.errors?.['maxlength'] ? 'RealName长度最多30位' : '';
      case 'email':
        return this.email?.errors?.['required'] ? 'Email必填' :
          this.email?.errors?.['minlength'] ? 'Email长度最少位' :
            this.email?.errors?.['maxlength'] ? 'Email长度最多100位' : '';
      case 'emailConfirmed':
        return this.emailConfirmed?.errors?.['required'] ? 'EmailConfirmed必填' :
          this.emailConfirmed?.errors?.['minlength'] ? 'EmailConfirmed长度最少位' :
            this.emailConfirmed?.errors?.['maxlength'] ? 'EmailConfirmed长度最多位' : '';
      case 'phoneNumber':
        return this.phoneNumber?.errors?.['required'] ? 'PhoneNumber必填' :
          this.phoneNumber?.errors?.['minlength'] ? 'PhoneNumber长度最少位' :
            this.phoneNumber?.errors?.['maxlength'] ? 'PhoneNumber长度最多20位' : '';
      case 'phoneNumberConfirmed':
        return this.phoneNumberConfirmed?.errors?.['required'] ? 'PhoneNumberConfirmed必填' :
          this.phoneNumberConfirmed?.errors?.['minlength'] ? 'PhoneNumberConfirmed长度最少位' :
            this.phoneNumberConfirmed?.errors?.['maxlength'] ? 'PhoneNumberConfirmed长度最多位' : '';
      case 'twoFactorEnabled':
        return this.twoFactorEnabled?.errors?.['required'] ? 'TwoFactorEnabled必填' :
          this.twoFactorEnabled?.errors?.['minlength'] ? 'TwoFactorEnabled长度最少位' :
            this.twoFactorEnabled?.errors?.['maxlength'] ? 'TwoFactorEnabled长度最多位' : '';
      case 'lockoutEnd':
        return this.lockoutEnd?.errors?.['required'] ? 'LockoutEnd必填' :
          this.lockoutEnd?.errors?.['minlength'] ? 'LockoutEnd长度最少位' :
            this.lockoutEnd?.errors?.['maxlength'] ? 'LockoutEnd长度最多位' : '';
      case 'lockoutEnabled':
        return this.lockoutEnabled?.errors?.['required'] ? 'LockoutEnabled必填' :
          this.lockoutEnabled?.errors?.['minlength'] ? 'LockoutEnabled长度最少位' :
            this.lockoutEnabled?.errors?.['maxlength'] ? 'LockoutEnabled长度最多位' : '';
      case 'accessFailedCount':
        return this.accessFailedCount?.errors?.['required'] ? 'AccessFailedCount必填' :
          this.accessFailedCount?.errors?.['minlength'] ? 'AccessFailedCount长度最少位' :
            this.accessFailedCount?.errors?.['maxlength'] ? 'AccessFailedCount长度最多位' : '';
      case 'lastLoginTime':
        return this.lastLoginTime?.errors?.['required'] ? 'LastLoginTime必填' :
          this.lastLoginTime?.errors?.['minlength'] ? 'LastLoginTime长度最少位' :
            this.lastLoginTime?.errors?.['maxlength'] ? 'LastLoginTime长度最多位' : '';
      case 'retryCount':
        return this.retryCount?.errors?.['required'] ? 'RetryCount必填' :
          this.retryCount?.errors?.['minlength'] ? 'RetryCount长度最少位' :
            this.retryCount?.errors?.['maxlength'] ? 'RetryCount长度最多位' : '';
      case 'avatar':
        return this.avatar?.errors?.['required'] ? 'Avatar必填' :
          this.avatar?.errors?.['minlength'] ? 'Avatar长度最少位' :
            this.avatar?.errors?.['maxlength'] ? 'Avatar长度最多200位' : '';
      case 'sex':
        return this.sex?.errors?.['required'] ? 'Sex必填' :
          this.sex?.errors?.['minlength'] ? 'Sex长度最少位' :
            this.sex?.errors?.['maxlength'] ? 'Sex长度最多位' : '';

      default:
        return '';
    }
  }
  edit(): void {
    if(this.formGroup.valid) {
      this.updateData = this.formGroup.value as SystemUserUpdateDto;
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
