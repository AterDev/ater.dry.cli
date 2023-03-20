// 该文件自动生成，会被覆盖更新
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumText'
})
export class EnumTextPipe implements PipeTransform {
  transform(value: unknown, type: string): unknown {
    let result = '';
    switch (type) {
      ${EnumBlocks}
      default:
        break;
    }
    return result;
  }
}