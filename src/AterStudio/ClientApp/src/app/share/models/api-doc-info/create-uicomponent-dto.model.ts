import { UIType } from '../enum/uitype.model';
import { ComponentType } from '../enum/component-type.model';
import { EntityInfo } from '../entity-info.model';
export interface CreateUIComponentDto {
  uiType?: UIType | null;
  componentType?: ComponentType | null;
  /**
   * 实体
   */
  modelInfo?: EntityInfo | null;
  serviceName: string;

}
