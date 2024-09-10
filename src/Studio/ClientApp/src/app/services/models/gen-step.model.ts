import { GenStepType } from '../enum/models/gen-step-type.model';
import { GenAction } from '../models/gen-action.model';
import { Project } from '../models/project.model';
export interface GenStep {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  templateContent?: string | null;
  path?: string | null;
  command?: string | null;
  genStepType?: GenStepType | null;
  genActions?: GenAction[];
  project?: Project | null;
  projectId: string;

}
