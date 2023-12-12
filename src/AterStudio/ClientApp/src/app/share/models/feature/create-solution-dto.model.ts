import { DBType } from '../enum/dbtype.model';
import { CacheType } from '../enum/cache-type.model';
import { ProjectType } from '../enum/project-type.model';
/**
 * 创建解决方案dto
 */
export interface CreateSolutionDto {
  /**
   * 名称
   */
  name: string;
  /**
   * 路径
   */
  path: string;
  dbType?: DBType | null;
  cacheType?: CacheType | null;
  /**
   * 是否包含租户
   */
  hasTenant: boolean;
  /**
   * 是否包含验证授权服务
   */
  hasIdentityServer: boolean;
  /**
   * 是否包含任务管理服务
   */
  hasTaskManager: boolean;
  /**
   * 写数据库连接字符串
   */
  commandDbConnStrings?: string | null;
  /**
   * 读数据库连接字符串
   */
  queryDbConnStrings?: string | null;
  /**
   * 缓存连接字符串
   */
  cacheConnStrings?: string | null;
  /**
   * 缓存实例名称
   */
  cacheInstanceName?: string | null;
  /**
   * 系统管理模块
   */
  hasSystemFeature: boolean;
  /**
   * 内容管理模块
   */
  hasCmsFeature: boolean;
  /**
   * 用户文件模块
   */
  hasFileManagerFeature: boolean;
  /**
   * 订单模块
   */
  hasOrderFeature: boolean;
  projectType?: ProjectType | null;

}
