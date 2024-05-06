import { SolutionType } from '../../enum/models/solution-type.model';
import { EntityInfo } from '../../models/entity-info.model';
import { ApiDocInfo } from '../../api-doc-info/models/api-doc-info.model';
import { ConfigData } from '../../advance/models/config-data.model';
import { TemplateFile } from '../../project/models/template-file.model';
/**
 * 项目
 */
export interface Project {
  id: string;
  createdTime: Date;
  updatedTime: Date;
  isDeleted: boolean;
  /**
   * 项目名称
   */
  name: string;
  /**
   * 显示名
   */
  displayName: string;
  /**
   * 路径
   */
  path: string;
  /**
   * 版本
   */
  version?: string | null;
  solutionType?: SolutionType | null;
  /**
   * Front Path
   */
  frontPath?: string | null;
  entityInfos?: EntityInfo[];
  apiDocInfos?: ApiDocInfo[];
  configDatas?: ConfigData[];
  templateFiles?: TemplateFile[];

}
