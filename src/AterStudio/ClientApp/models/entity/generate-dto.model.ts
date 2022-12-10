import { CommandType } from '../enum/command-type.model';
export interface GenerateDto {
  projectId: number;
  entityPath?: string | null;
  commandType?: CommandType | null;

}
