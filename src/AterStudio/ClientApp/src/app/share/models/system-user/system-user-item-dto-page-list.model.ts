import { SystemUserItemDto } from '../system-user/system-user-item-dto.model';
export interface SystemUserItemDtoPageList {
  count: number;
  data?: SystemUserItemDto[] | null;
  pageIndex: number;

}
