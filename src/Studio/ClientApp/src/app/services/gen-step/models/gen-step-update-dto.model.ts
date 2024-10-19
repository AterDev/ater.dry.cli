import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step更新时DTO
 */
export interface GenStepUpdateDto {
  /**
   * 模板或命令内容
   */
  content?: string | null;
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

}
