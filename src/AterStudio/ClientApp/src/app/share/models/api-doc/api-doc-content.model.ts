import { RestApiInfo } from '../rest-api-info.model';
import { EntityInfo } from '../entity-info.model';
import { ApiDocTag } from '../api-doc-tag.model';
/**
 * 接口返回模型
 */
export interface ApiDocContent {
  /**
   * 接口信息
   */
  restApiInfos?: RestApiInfo[] | null;
  /**
   * 所有请求及返回类型信息
   */
  modelInfos?: EntityInfo[] | null;
  /**
   * tag信息
   */
  openApiTags?: ApiDocTag[] | null;

}
