/**
 * 模板内容
 */
export interface TemplateFile {
  id: string;
  projectId: string;
  /**
   * 名称
   */
  name: string;
  /**
   * 显示名称
   */
  displayName?: string | null;
  /**
   * 内容
   */
  content?: string | null;

}
