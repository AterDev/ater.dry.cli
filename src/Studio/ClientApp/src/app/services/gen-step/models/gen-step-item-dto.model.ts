import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step列表元素
 */
export interface GenStepItemDto {
  /**
   * 步骤名称
   */
  name: string;
  genStepType?: GenStepType | null;
  id: string;
  createdTime: Date;

}
