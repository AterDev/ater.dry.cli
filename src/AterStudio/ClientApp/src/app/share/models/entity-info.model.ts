import { EntityKeyType } from './enum/entity-key-type.model';
import { Project } from './project/project.model';
import { PropertyInfo } from './property-info.model';
/**
 * 实体
 */
export interface EntityInfo {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
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
   * 项目
   */
  project?: Project | null;
  projectId: string;
  /**
   * 属性
   */
  propertyInfos?: PropertyInfo[];

}
