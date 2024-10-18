import { GenSourceType } from '../../enum/models/gen-source-type.model';
/**
 * The project's generate action列表元素
 */
export interface GenActionItemDto {
  /**
   * action name
   */
  name: string;
  sourceType?: GenSourceType | null;
  id: string;
  createdTime: Date;

}
