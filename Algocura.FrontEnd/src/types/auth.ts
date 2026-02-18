export type PlatformRole =
  | "Administrator"
  | "Scientist"
  | "Reviewer"
  | "Service";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface TokenResponse {
  accessToken: string;
  expiresAtUtc?: string;
}

export interface JwtPrincipal {
  sub?: string;
  email?: string;
  name?: string;
  roles: PlatformRole[];
  exp?: number;
}
