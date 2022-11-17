export interface AuthResult {
  id: string;
  /**
   * 用户名
   */
  username?: string | null;
  role?: string | null;
  /**
   * token
   */
  token?: string | null;

}
