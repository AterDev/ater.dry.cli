export interface UpdateConfigOptionsDto {
  /**
   * dto项目目录
   */
  dtoPath?: string | null;
  entityPath?: string | null;
  storePath?: string | null;
  apiPath?: string | null;
  idType?: string | null;
  createdTimeName?: string | null;
  updatedTimeName?: string | null;
  isSplitController?: boolean | null;

}
