import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren
} from "react";
import { login as loginRequest } from "../api/identityApi";
import { tokenStorage } from "../api/httpClient";
import type {
  JwtPrincipal,
  LoginRequest,
  PlatformRole
} from "../types/auth";
import { isExpired, parseJwt } from "./jwt";

interface AuthContextValue {
  token: string | null;
  principal: JwtPrincipal | null;
  isAuthenticated: boolean;
  login: (request: LoginRequest) => Promise<void>;
  logout: () => void;
  hasAnyRole: (roles: readonly string[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function loadPrincipal(): {
  token: string | null;
  principal: JwtPrincipal | null;
} {
  const token = tokenStorage.get();
  if (!token) {
    return { token: null, principal: null };
  }

  try {
    const principal = parseJwt(token);
    if (isExpired(principal)) {
      tokenStorage.clear();
      return { token: null, principal: null };
    }

    return { token, principal };
  } catch {
    tokenStorage.clear();
    return { token: null, principal: null };
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const initial = loadPrincipal();
  const [token, setToken] = useState<string | null>(initial.token);
  const [principal, setPrincipal] = useState<JwtPrincipal | null>(
    initial.principal
  );

  const logout = useCallback(() => {
    tokenStorage.clear();
    setToken(null);
    setPrincipal(null);
  }, []);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await loginRequest(request);
    tokenStorage.set(response.accessToken);

    const parsed = parseJwt(response.accessToken);
    setToken(response.accessToken);
    setPrincipal(parsed);
  }, []);

  const hasAnyRole = useCallback(
    (roles: readonly string[]) => {
      if (!principal) return false;
      return principal.roles.some((role: PlatformRole) =>
        roles.includes(role)
      );
    },
    [principal]
  );

  useEffect(() => {
    const handler = () => logout();
    window.addEventListener("algocura:unauthorized", handler);
    return () =>
      window.removeEventListener("algocura:unauthorized", handler);
  }, [logout]);

  const value = useMemo<AuthContextValue>(
    () => ({
      token,
      principal,
      isAuthenticated: Boolean(token && principal),
      login,
      logout,
      hasAnyRole
    }),
    [token, principal, login, logout, hasAnyRole]
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
