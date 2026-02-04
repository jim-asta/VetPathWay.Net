export interface AuthToken {
  accessToken: string;
  idToken?: string;
  refreshToken?: string;
  expiresIn?: number;
}
