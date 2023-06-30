export interface EntityFile {
  name: string;
  /**
   * 注释说明
   */
  comment?: string | null;
  baseDirPath: string;
  path: string;
  content?: string | null;
  hasDto: boolean;
  hasManager: boolean;
  hasAPI: boolean;

}
