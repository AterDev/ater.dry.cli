import { ValueType } from '../enum/value-type.model';
/**
 * 配置
 */
export interface ConfigData {
  id: string;
  /**
   * 键
   */
  key: string;
  /**
   * 值
   */
  value: string;
  valueType?: ValueType | null;

}
