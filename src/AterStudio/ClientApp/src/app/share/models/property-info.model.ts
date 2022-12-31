import { EntityInfo } from './entity-info.model';
export interface PropertyInfo {
  id: string;
  projectId: string;
  type?: string | null;
  name?: string | null;
  displayName?: string | null;
  isList: boolean;
  isPublic: boolean;
  isNavigation: boolean;
  navigationName?: string | null;
  hasMany?: boolean | null;
  isEnum: boolean;
  attributeText?: string | null;
  commentXml?: string | null;
  commentSummary?: string | null;
  isRequired: boolean;
  isNullable: boolean;
  minLength?: number | null;
  maxLength?: number | null;
  isDecimal: boolean;
  suffixContent?: string | null;
  entityInfo?: EntityInfo | null;

}
