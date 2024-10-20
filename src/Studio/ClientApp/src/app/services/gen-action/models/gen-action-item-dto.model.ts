import { GenSourceType } from '../../enum/models/gen-source-type.model';
import { Variable } from '../../models/variable.model';
/**
 * The project's generate action列表元素
 */
export interface GenActionItemDto {
  /**
   * action name
   */
  name: string;
  description?: string | null;
  sourceType?: GenSourceType | null;
  id: string;
  createdTime: Date;
  variables?: Variable[];

}
