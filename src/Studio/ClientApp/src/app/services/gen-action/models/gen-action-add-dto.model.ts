import { GenSourceType } from '../../enum/models/gen-source-type.model';
/**
 * The project's generate action添加时DTO
 */
export interface GenActionAddDto {
  /**
   * action name
   */
  name: string;
  description?: string | null;
  sourceType?: GenSourceType | null;
  projectId: string;

}
