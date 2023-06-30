import { DBType } from '../enum/dbtype.model';
import { CacheType } from '../enum/cache-type.model';
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
   * 内容管理模块
   */
  hasCmsFeature: boolean;
  /**
   * 用户日志模块
   */
  hasUserLogsFeature: boolean;
  /**
   * 系统日志模块
   */
  hasSystemLogsFeature: boolean;

}
