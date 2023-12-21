import { CommandType } from '../enum/command-type.model';
export interface BatchGenerateDto {
  projectId: string;
  entityPaths: string[];
  /**
   * 服务
   */
  serviceName?: string | null;
  /**
   * 命令类型
   */
  commandType?: CommandType | null;
  /**
   * 项目路径
   */
  projectPath?: string[] | null;
  force: boolean;

}
