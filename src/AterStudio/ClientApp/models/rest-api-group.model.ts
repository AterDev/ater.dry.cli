import { RestApiInfo } from './rest-api-info.model';
export interface RestApiGroup {
  name?: string | null;
  description?: string | null;
  apiInfos?: RestApiInfo[] | null;

}
