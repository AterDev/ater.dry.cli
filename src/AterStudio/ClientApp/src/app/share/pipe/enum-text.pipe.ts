// 该文件自动生成，会被覆盖更新
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): unknown {
    let result = '';
    switch (type) {
      case 'CacheType':
{
  switch (value)
  {
    case 0: result = 'Redis'; break;
    case 1: result = 'Memory'; break;
    case 2: result = 'None'; break;
    default: '默认'; break;
  }
}
break;
case 'DBType':
{
  switch (value)
  {
    case 0: result = 'SQLServer'; break;
    case 1: result = 'PostgreSQL'; break;
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