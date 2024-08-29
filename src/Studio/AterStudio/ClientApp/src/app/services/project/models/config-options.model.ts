import { ControllerType } from '../../enum/models/controller-type.model';
import { SolutionType } from '../../enum/models/solution-type.model';
/**
 * 项目配置
 */
export interface ConfigOptions {
  /**
   * 项目根目录
   */
  rootPath: string;
  /**
   * 是否为轻量项目
   */
  isLight: boolean;
  projectId: string;
  /**
   * dto项目目录
   */
  dtoPath: string;
  entityPath: string;
  dbContextPath: string;
  applicationPath: string;
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
  controllerType?: ControllerType | null;
  version: string;
  /**
   * 前端路径
   */
  webAppPath?: string | null;
  solutionType?: SolutionType | null;

}
