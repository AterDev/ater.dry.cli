import { EntityInfo } from '../models/entity-info.model';
/**
 * 属性
 */
export interface PropertyInfo {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  /**
   * 类型
   */
  type: string;
  /**
   * 名称
   */
  name: string;
  displayName?: string | null;
  /**
   * 是否是数组
   */
  isList: boolean;
  isPublic: boolean;
  /**
   * 是否为导航属性
   */
  isNavigation: boolean;
  isJsonIgnore: boolean;
  /**
   * 导航属性类名称
   */
  navigationName?: string | null;
  isComplexType: boolean;
  /**
   * 导航属性的对应关系
   */
  hasMany?: boolean | null;
  isEnum: boolean;
  /**
   * 是否包括set方法
   */
  hasSet: boolean;
  attributeText?: string | null;
  /**
   * xml comment
   */
  commentXml?: string | null;
  /**
   * comment summary
   */
  commentSummary?: string | null;
  /**
   * 是否必须
   */
  isRequired: boolean;
  /**
   * 可空?
   */
  isNullable: boolean;
  minLength?: number | null;
  maxLength?: number | null;
  isDecimal: boolean;
  /**
   * 尾缀，如#endregion
   */
  suffixContent?: string | null;
  /**
   * 默认值
   */
  defaultValue: string;
  /**
   * 实体
   */
  entityInfo?: EntityInfo | null;
  entityInfoId: string;

}
