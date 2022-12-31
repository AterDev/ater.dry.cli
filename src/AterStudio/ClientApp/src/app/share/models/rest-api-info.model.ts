import { OperationType } from './enum/operation-type.model';
import { PropertyInfo } from './property-info.model';
import { EntityInfo } from './entity-info.model';
export interface RestApiInfo {
  operationType?: OperationType | null;
  router?: string | null;
  summary?: string | null;
  tag?: string | null;
  operationId?: string | null;
  queryParameters?: PropertyInfo[] | null;
  requestInfo?: EntityInfo | null;
  responseInfo?: EntityInfo | null;

}
