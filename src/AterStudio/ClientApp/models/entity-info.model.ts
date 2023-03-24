import { EntityKeyType } from './enum/entity-key-type.model';
import { PropertyInfo } from './property-info.model';
export interface EntityInfo {
  id: string;
  projectId: string;
  name?: string | null;
  namespaceName?: string | null;
  assemblyName?: string | null;
  comment?: string | null;
  keyType?: EntityKeyType | null;
  isEnum?: boolean | null;
  propertyInfos?: PropertyInfo[] | null;

}
