import { DBType } from '../enum/dbtype.model';
import { CacheType } from '../enum/cache-type.model';
/**
 * 创建解决方案dto
 */
export interface CreateSolutionDto {
  /**
   * 名称
   */
  name?: string | null;
  /**
   * 路径
   */
  path?: string | null;
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
   * 功能模块
   */
  features?: string[] | null;

}
