export interface ConfigOptions {
  rootPath?: string | null;
  projectId: string;
  dtoPath?: string | null;
  entityPath?: string | null;
  dbContextPath?: string | null;
  storePath?: string | null;
  apiPath?: string | null;
  idStyle?: string | null;
  idType?: string | null;
  createdTimeName?: string | null;
  updatedTimeName?: string | null;
  isSplitController?: boolean | null;
  version: number;
  swaggerPath?: string | null;
  webAppPath?: string | null;

}
