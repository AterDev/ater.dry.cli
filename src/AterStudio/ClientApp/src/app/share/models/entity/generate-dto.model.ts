import { CommandType } from '../enum/command-type.model';
export interface GenerateDto {
  projectId: string;
  entityPath?: string | null;
  commandType?: CommandType | null;

}
