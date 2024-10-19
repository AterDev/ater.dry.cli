import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step列表元素
 */
export interface GenStepItemDto {
  /**
   * 步骤名称
   */
  name: string;
  /**
   * 模板或脚本路径
   */
  path?: string | null;
  /**
   * 输出路径
   */
  outputPath?: string | null;
  genStepType?: GenStepType | null;
  id: string;
  createdTime: Date;

}
