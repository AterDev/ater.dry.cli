import { UIType } from '../enum/uitype.model';
import { ComponentType } from '../enum/component-type.model';
import { EntityInfo } from '../entity-info.model';
/**
 * 生成组件模型
 */
export interface CreateUIComponentDto {
  /**
   * 使用的UI组件库
   */
  uiType?: UIType | null;
  componentType?: ComponentType | null;
  /**
   * defined entity model
   */
  modelInfo?: EntityInfo | null;
  serviceName: string;

}
