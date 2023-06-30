export interface EntityFile {
  name?: string | null;
  /**
   * 注释说明
   */
  comment?: string | null;
  baseDirPath?: string | null;
  path?: string | null;
  content?: string | null;
  hasDto: boolean;
  hasManager: boolean;
  hasAPI: boolean;

}
