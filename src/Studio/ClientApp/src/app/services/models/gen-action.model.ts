import { GenSourceType } from '../enum/models/gen-source-type.model';
import { GenStep } from '../models/gen-step.model';
import { Project } from '../models/project.model';
export interface GenAction {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  name: string;
  description?: string | null;
  sourceType?: GenSourceType | null;
  genSteps?: GenStep[];
  project?: Project | null;
  projectId: string;

}
