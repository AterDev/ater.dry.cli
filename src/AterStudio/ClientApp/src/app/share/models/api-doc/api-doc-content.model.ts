import { RestApiGroup } from '../rest-api-group.model';
import { EntityInfo } from '../entity-info.model';
import { ApiDocTag } from '../api-doc-tag.model';
/**
 * 接口返回模型
 */
export interface ApiDocContent {
  /**
   * 接口信息
   */
  restApiGroups?: RestApiGroup[];
  /**
   * 所有请求及返回类型信息
   */
  modelInfos?: EntityInfo[];
  /**
   * tag信息
   */
  openApiTags?: ApiDocTag[];

}
