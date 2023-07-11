import { SolutionType } from '../enum/solution-type.model';
/**
 * 项目
 */
export interface Project {
  id: string;
  projectId: string;
  name: string;
  displayName: string;
  path: string;
  version?: string | null;
  solutionType?: SolutionType | null;

}
