import { UIType } from '../enum/uitype.model';
import { EntityInfo } from '../entity-info.model';
/**
 * 生成组件模型
 */
export interface CreateUIComponentDto {
  /**
   * 使用的UI组件库
   */
  uiType?: UIType | null;
  modelInfo?: EntityInfo | null;
  serviceName?: string | null;

}
