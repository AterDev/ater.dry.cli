// 该文件自动生成，会被覆盖更新
import { Injectable, Pipe, PipeTransform } from '@angular/core';

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
            switch (value)
        {
            case 0: result = 'Redis'; break;
            default: '默认'; break;
          }
        }
        break;
      case 'DBType':
        {
            switch (value)
        {
            case 0: result = 'PostgreSQL'; break;
            default: '默认'; break;
          }
        }
        break;

      default:
        break;
    }
    return result;
  }
}