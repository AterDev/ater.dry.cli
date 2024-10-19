import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step列表元素
 */
export interface GenStepItemDto {
  /**
   * 模板或命令内容
   */
  content?: string | null;
  /**
   * 生成内容
   */
  outputContent: string;
  /**
   * 模板或脚本路径
   */
  path?: string | null;
  /**
   * 输出路径
   */
  outputPath?: string | null;
  /**
   * command content
   */
  command?: string | null;
  genStepType?: GenStepType | null;
  id: string;
  createdTime: Date;

}
