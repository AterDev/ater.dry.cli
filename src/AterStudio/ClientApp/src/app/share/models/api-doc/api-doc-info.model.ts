import { Project } from '../project/project.model';
/**
 * 接口文档
 */
export interface ApiDocInfo {
  id: string;
  /**
   * 文档名称
   */
  name: string;
  /**
   * 文档描述
   */
  description?: string | null;
  /**
   * 文档地址
   */
  path: string;
  /**
   * 项目
   */
  project?: Project | null;
  projectId: string;

}
