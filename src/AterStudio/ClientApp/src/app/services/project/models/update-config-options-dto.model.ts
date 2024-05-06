import { ControllerType } from '../../enum/models/controller-type.model';
export interface UpdateConfigOptionsDto {
  dtoPath?: string | null;
  entityPath?: string | null;
  entityFrameworkPath?: string | null;
  storePath?: string | null;
  apiPath?: string | null;
  idType?: string | null;
  createdTimeName?: string | null;
  updatedTimeName?: string | null;
  controllerType?: ControllerType | null;
  isSplitController?: boolean | null;

}
