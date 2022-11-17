/**
 * 角色更新时请求结构
 */
export interface SystemRoleUpdateDto {
  /**
   * 角色显示名称
   */
  name?: string | null;
  /**
   * 角色名，系统标识
   */
  nameValue?: string | null;
  /**
   * 是否系统内置,系统内置不可删除
   */
  isSystem?: boolean | null;
  /**
   * 图标
   */
  icon?: string | null;

}
