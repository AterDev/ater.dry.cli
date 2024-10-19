// 该文件自动生成，会被覆盖更新
import { Injectable, NgModule, Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
@Injectable({ providedIn: 'root' })
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): string {
    let result = '';
    switch (type) {
      case 'CacheType':
        {
          switch (value) {
            case 0: result = 'Redis'; break;
            case 1: result = 'Memory'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'CommandType':
        {
          switch (value) {
            case 0: result = 'dto'; break;
            case 1: result = 'manager'; break;
            case 2: result = 'api'; break;
            case 3: result = 'protobuf'; break;
            case 4: result = 'clear'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'ComponentType':
        {
          switch (value) {
            case 0: result = '提交表单'; break;
            case 1: result = '展示表格'; break;
            case 2: result = '详情字段'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'ControllerType':
        {
          switch (value) {
            case 0: result = '用户端'; break;
            case 1: result = '管理端'; break;
            case 2: result = '用户端和管理端'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'DBType':
        {
          switch (value) {
            case 0: result = 'PostgreSQL'; break;
            case 1: result = 'SQLServer'; break;
            case 2: result = 'SQLite'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'FrontType':
        {
          switch (value) {
            case 1: result = 'Angular'; break;
            case 2: result = 'Blazor'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'GenSourceType':
        {
          switch (value) {
            case 0: result = 'Entity Class'; break;
            case 1: result = 'OpenAPI'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'GenStepType':
        {
          switch (value) {
            case 0: result = '模板生成'; break;
            case 1: result = '运行命令'; break;
            case 2: result = '运行脚本'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'OperationType':
        {
          switch (value) {
            case 0: result = 'get'; break;
            case 1: result = 'put'; break;
            case 2: result = 'post'; break;
            case 3: result = 'delete'; break;
            case 4: result = 'options'; break;
            case 5: result = 'head'; break;
            case 6: result = 'patch'; break;
            case 7: result = 'trace'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'RequestLibType':
        {
          switch (value) {
            case 0: result = 'angular http'; break;
            case 1: result = 'axios'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'SolutionType':
        {
          switch (value) {
            case 0: result = 'DotNet'; break;
            case 1: result = 'Node'; break;
            case 2: result = 'Empty'; break;
            default: result = '默认'; break;
          }
        }
        break;
      case 'StringConvertType':
        {
          switch (value) {
            case 0: result = 'Guid'; break;
            case 1: result = '命名转换'; break;
            case 2: result = '编码'; break;
            case 3: result = '解码'; break;
            case 4: result = '加密'; break;
            default: result = '默认'; break;
          }
        }
        break;

      default:
        break;
    }
    return result;
  }
}


@NgModule({
  declarations: [EnumTextPipe], exports: [EnumTextPipe]
})
export class EnumTextPipeModule { }
