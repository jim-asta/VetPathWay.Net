export interface AuthToken {
  access_token: string;
  id_token?: string;
  refresh_token?: string;
  expires_in?: number;
}
