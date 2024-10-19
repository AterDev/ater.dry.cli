import { GenStepType } from '../../enum/models/gen-step-type.model';
/**
 * task step筛选条件
 */
export interface GenStepFilterDto {
  pageIndex: number;
  pageSize: number;
  orderBy?: any | null;
  genStepType?: GenStepType | null;
  projectId?: string | null;
  genActionId?: string | null;

}
