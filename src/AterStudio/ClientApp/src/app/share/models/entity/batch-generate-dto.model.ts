import { CommandType } from '../enum/command-type.model';
export interface BatchGenerateDto {
  projectId: string;
  entityPaths?: string[] | null;
  commandType?: CommandType | null;
  projectPath?: string[] | null;
  force: boolean;

}
