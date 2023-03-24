import { ProjectType } from '../enum/project-type.model';
/**
 * 项目信息
 */
export interface SubProjectInfo {
  name?: string | null;
  path?: string | null;
  projectType?: ProjectType | null;

}
