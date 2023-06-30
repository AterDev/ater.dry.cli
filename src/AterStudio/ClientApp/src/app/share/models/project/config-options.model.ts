/**
 * 项目配置
 */
export interface ConfigOptions {
  /**
   * 项目根目录
   */
  rootPath?: string | null;
  projectId: string;
  /**
   * dto项目目录
   */
  dtoPath?: string | null;
  entityPath?: string | null;
  dbContextPath?: string | null;
  storePath?: string | null;
  apiPath?: string | null;
  /**
   * NameId/Id
   */
  idStyle?: string | null;
  idType?: string | null;
  createdTimeName?: string | null;
  updatedTimeName?: string | null;
  /**
   * 是否拆分
   */
  isSplitController?: boolean | null;
  version?: string | null;
  /**
   * swagger地址
   */
  swaggerPath?: string | null;
  /**
   * 前端路径
   */
  webAppPath?: string | null;

}
