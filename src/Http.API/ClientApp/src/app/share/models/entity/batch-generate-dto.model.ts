import { CommandType } from '../enum/command-type.model';
export interface BatchGenerateDto {
  projectId: number;
  entityPaths?: string[] | null;
  commandType?: CommandType | null;

}
