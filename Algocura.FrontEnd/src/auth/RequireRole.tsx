import type { PropsWithChildren } from "react";
import { Navigate } from "react-router";
import { useAuth } from "./AuthContext";

interface RequireRoleProps extends PropsWithChildren {
  roles: readonly string[];
}

export function RequireRole({
  roles,
  children
}: RequireRoleProps) {
  const auth = useAuth();

  if (!auth.hasAnyRole(roles)) {
    return <Navigate to="/forbidden" replace />;
  }

  return children;
}
