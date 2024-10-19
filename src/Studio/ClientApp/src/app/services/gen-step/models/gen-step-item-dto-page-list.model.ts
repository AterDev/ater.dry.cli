import { GenStepItemDto } from '../../gen-step/models/gen-step-item-dto.model';
export interface GenStepItemDtoPageList {
  count: number;
  data?: GenStepItemDto[];
  pageIndex: number;

}
