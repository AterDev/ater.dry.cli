import { RestApiInfo } from './rest-api-info.model';
/**
 * 接口分组信息
 */
export interface RestApiGroup {
  name?: string | null;
  description?: string | null;
  apiInfos?: RestApiInfo[] | null;

}
