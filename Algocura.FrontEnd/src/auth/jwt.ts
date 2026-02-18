import type { JwtPrincipal, PlatformRole } from "../types/auth";

const roleClaim =
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

const nameClaim =
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

function decodeBase64Url(value: string): string {
  const padded = value.replace(/-/g, "+").replace(/_/g, "/");
  const padding = "=".repeat((4 - (padded.length % 4)) % 4);
  return decodeURIComponent(
    Array.from(atob(padded + padding))
      .map((character) =>
        `%${character.charCodeAt(0).toString(16).padStart(2, "0")}`
      )
      .join("")
  );
}

export function parseJwt(token: string): JwtPrincipal {
  const parts = token.split(".");
  if (parts.length !== 3) {
    throw new Error("Invalid JWT.");
  }

  const payload = JSON.parse(decodeBase64Url(parts[1])) as Record<
    string,
    unknown
  >;

  const rawRoles = payload[roleClaim] ?? payload.role ?? payload.roles;
  const roles = (
    Array.isArray(rawRoles)
      ? rawRoles
      : rawRoles
        ? [rawRoles]
        : []
  ).filter((role): role is PlatformRole =>
    ["Administrator", "Scientist", "Reviewer", "Service"].includes(
      String(role)
    )
  );

  return {
    sub: String(payload.sub ?? ""),
    email: String(payload.email ?? ""),
    name: String(payload[nameClaim] ?? payload.name ?? ""),
    roles,
    exp: typeof payload.exp === "number" ? payload.exp : undefined
  };
}

export function isExpired(principal: JwtPrincipal): boolean {
  return principal.exp
    ? principal.exp * 1000 <= Date.now()
    : false;
}
