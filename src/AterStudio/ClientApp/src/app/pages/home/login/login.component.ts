import { Component, OnInit } from '@angular/core';
// import { OAuthService, OAuthErrorEvent, UserInfo } from 'angular-oauth2-oidc';
import { Router } from '@angular/router';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { LoginService } from 'src/app/auth/login.service';
import { AuthService } from 'src/app/share/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  public loginForm!: FormGroup;
  constructor(
    private loginService: LoginService,
    private authService: AuthService,
    private router: Router

  ) {
  }
  get username() { return this.loginForm.get('username'); }
  get password() { return this.loginForm.get('password'); }
  ngOnInit(): void {
    // const token = this.oauthService.getAccessToken();
    // const cliams = this.oauthService.getIdentityClaims();
    // if (token && cliams) {
    //   this.router.navigateByUrl('/index');
    // }

    // this.oauthService.events.subscribe(event => {
    //   if (event instanceof OAuthErrorEvent) {
    //     // TODO:处理错误
    //     console.error(event);
    //   } else {
    //     if (event.type === 'token_received' || event.type === 'token_refreshed') {
    //       this.oauthService.loadUserProfile()
    //         .then(() => {
    //           this.router.navigateByUrl('/index');
    //         });
    //     }
    //   }
    // });

    this.loginForm = new FormGroup({
      username: new FormControl('', [Validators.required, Validators.minLength(3)]),
      password: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(50)])
    });
  }

  /**
   * 错误信息
   * @param type 字段名称
   */
  getValidatorMessage(type: string): string {
    switch (type) {
      case 'username':
        return this.username?.errors?.['required'] ? '用户名必填' :
          this.username?.errors?.['minlength']
            || this.username?.errors?.['maxlength'] ? '用户名长度3-20位' : '';
      case 'password':
        return this.password?.errors?.['required'] ? '密码必填' :
          this.password?.errors?.['minlength'] ? '密码长度不可低于6位' :
            this.password?.errors?.['maxlength'] ? '密码长度不可超过50' : '';
      default:
        break;
    }
    return '';
  }
  doLogin(): void {

    let data = this.loginForm.value;
    // 登录接口
    this.authService.login(data)
      .subscribe(res => {
        this.loginService.saveLoginState(res);
        this.router.navigate(['/']);
      });
  }


  logout(): void {
    this.loginService.logout();
  }
}
