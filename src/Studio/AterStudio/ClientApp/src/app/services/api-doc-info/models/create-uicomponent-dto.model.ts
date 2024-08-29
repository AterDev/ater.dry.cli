import { UIType } from '../../enum/models/uitype.model';
import { ComponentType } from '../../enum/models/component-type.model';
import { EntityInfo } from '../../models/entity-info.model';
export interface CreateUIComponentDto {
  uiType?: UIType | null;
  componentType?: ComponentType | null;
  /**
   * 实体
   */
  modelInfo?: EntityInfo | null;
  serviceName: string;

}
