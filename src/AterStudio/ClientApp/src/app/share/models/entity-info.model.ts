import { EntityKeyType } from './enum/entity-key-type.model';
import { PropertyInfo } from './property-info.model';
/**
 * defined entity model
 */
export interface EntityInfo {
  id: string;
  projectId: string;
  /**
   * 类名
   */
  name: string;
  /**
   * 命名空间
   */
  namespaceName?: string | null;
  /**
   * 程序集名称
   */
  assemblyName?: string | null;
  /**
   * 类注释
   */
  comment?: string | null;
  /**
   * 类注释
   */
  summary?: string | null;
  keyType?: EntityKeyType | null;
  /**
   * 是否为枚举类
   */
  isEnum?: boolean | null;
  isList: boolean;
  /**
   * 属性
   */
  propertyInfos?: PropertyInfo[];

}
