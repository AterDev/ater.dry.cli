import { ProjectType } from '../enum/project-type.model';
export interface SubProjectInfo {
  name: string;
  path: string;
  projectType?: ProjectType | null;

}
