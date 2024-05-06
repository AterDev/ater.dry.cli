import { Project } from '../../project/models/project.model';
/**
 * 接口文档
 */
export interface ApiDocInfo {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
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
   * 生成路径
   */
  localPath?: string | null;
  /**
   * 项目
   */
  project?: Project | null;
  projectId: string;

}
