import { RestApiInfo } from './rest-api-info.model';
/**
 * 接口分组信息
 */
export interface RestApiGroup {
  name: string;
  description?: string | null;
  apiInfos?: RestApiInfo[];

}
