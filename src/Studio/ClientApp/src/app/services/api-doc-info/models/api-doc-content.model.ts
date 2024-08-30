import { RestApiGroup } from '../../models/rest-api-group.model';
import { EntityInfo } from '../../models/entity-info.model';
import { ApiDocTag } from '../../models/api-doc-tag.model';
export interface ApiDocContent {
  restApiGroups?: RestApiGroup[];
  modelInfos?: EntityInfo[];
  openApiTags?: ApiDocTag[];

}
