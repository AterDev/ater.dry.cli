import { CommandType } from '../enum/command-type.model';
export interface GenerateDto {
  projectId: string;
  entityPath?: string | null;
  /**
   * 命令类型
   */
  commandType?: CommandType | null;
  /**
   * 是否覆盖
   */
  force: boolean;

}
