import { SystemRoleItemDto } from '../system-role/system-role-item-dto.model';
export interface SystemRoleItemDtoPageList {
  count: number;
  data?: SystemRoleItemDto[] | null;
  pageIndex: number;

}
