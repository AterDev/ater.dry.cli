import { Project } from '../project/project.model';
/**
 * 模板内容
 */
export interface TemplateFile {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
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
  /**
   * 项目
   */
  project?: Project | null;
  projectId: string;

}
