import { SolutionType } from '../enum/solution-type.model';
/**
 * 项目配置
 */
export interface ConfigOptions {
  /**
   * 项目根目录
   */
  rootPath: string;
  projectId: string;
  /**
   * dto项目目录
   */
  dtoPath: string;
  entityPath: string;
  dbContextPath: string;
  storePath: string;
  apiPath: string;
  /**
   * NameId/Id
   */
  idStyle: string;
  idType: string;
  createdTimeName: string;
  updatedTimeName: string;
  /**
   * 控制器是否拆分
   */
  isSplitController?: boolean | null;
  version: string;
  /**
   * swagger地址
   */
  swaggerPath?: string | null;
  /**
   * 前端路径
   */
  webAppPath?: string | null;
  solutionType?: SolutionType | null;

}
