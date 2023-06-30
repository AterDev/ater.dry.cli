/**
 * 接口文档
 */
export interface ApiDocInfo {
  id: string;
  projectId: string;
  /**
   * 文档名称
   */
  name?: string | null;
  /**
   * 文档描述
   */
  description?: string | null;
  /**
   * 文档地址
   */
  path?: string | null;

}
