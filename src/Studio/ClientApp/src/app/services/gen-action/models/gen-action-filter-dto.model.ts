import { GenSourceType } from '../../enum/models/gen-source-type.model';
/**
 * The project's generate action筛选条件
 */
export interface GenActionFilterDto {
  pageIndex: number;
  pageSize: number;
  orderBy?: any | null;
  /**
   * action name
   */
  name?: string | null;
  sourceType?: GenSourceType | null;
  projectId?: string | null;

}
