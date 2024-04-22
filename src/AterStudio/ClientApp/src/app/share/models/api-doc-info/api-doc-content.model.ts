import { RestApiGroup } from '../rest-api-group.model';
import { EntityInfo } from '../entity-info.model';
import { ApiDocTag } from '../api-doc-tag.model';
export interface ApiDocContent {
  restApiGroups?: RestApiGroup[];
  modelInfos?: EntityInfo[];
  openApiTags?: ApiDocTag[];

}
