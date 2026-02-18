import type { LoginRequest, TokenResponse } from "../types/auth";
import { apiRequest } from "./httpClient";

const identityBaseUrl =
  import.meta.env.VITE_IDENTITY_API_URL ?? "http://localhost:7013";

export function login(request: LoginRequest): Promise<TokenResponse> {
  return apiRequest<TokenResponse>(
    identityBaseUrl,
    "/api/v1/auth/login",
    {
      method: "POST",
      body: JSON.stringify(request)
    }
  );
}

export function getCurrentUser(): Promise<Record<string, unknown>> {
  return apiRequest<Record<string, unknown>>(
    identityBaseUrl,
    "/api/v1/auth/me"
  );
}
