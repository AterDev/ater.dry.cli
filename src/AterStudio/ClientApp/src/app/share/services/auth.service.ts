import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { LoginDto } from '../models/auth/login-dto.model';
import { AuthResult } from '../models/auth/auth-result.model';

/**
 * 系统用户授权登录
 */
@Injectable({ providedIn: 'root' })
export class AuthService extends BaseService {
  /**
   * 登录获取Token
   * @param data LoginDto
   */
  login(data: LoginDto): Observable<AuthResult> {
    const url = `/api/Auth`;
    return this.request<AuthResult>('post', url, data);
  }

  /**
   * 退出
   * @param id string
   */
  logout(id: string): Observable<boolean> {
    const url = `/api/Auth/${id}`;
    return this.request<boolean>('get', url);
  }

}
