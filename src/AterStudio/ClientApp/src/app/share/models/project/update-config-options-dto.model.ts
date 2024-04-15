import { ControllerType } from '../enum/controller-type.model';
export interface UpdateConfigOptionsDto {
  /**
   * dto项目目录
   */
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
