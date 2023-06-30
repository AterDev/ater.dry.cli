import { ProjectType } from '../enum/project-type.model';
/**
 * 项目信息
 */
export interface SubProjectInfo {
  name: string;
  path: string;
  projectType?: ProjectType | null;

}
