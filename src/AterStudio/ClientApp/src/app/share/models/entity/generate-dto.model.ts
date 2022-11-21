import { CommandType } from '../enum/command-type.model';
export interface GenerateDto {
  entityPath?: string | null;
  commandType?: CommandType | null;

}
