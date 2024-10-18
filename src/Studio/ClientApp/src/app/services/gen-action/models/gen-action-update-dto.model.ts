import { GenSourceType } from '../../enum/models/gen-source-type.model';
/**
 * The project's generate action更新时DTO
 */
export interface GenActionUpdateDto {
  /**
   * action name
   */
  name?: string | null;
  description?: string | null;
  sourceType?: GenSourceType | null;

}
