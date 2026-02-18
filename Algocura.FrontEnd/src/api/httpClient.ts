const TOKEN_KEY = "algocura.accessToken";

export const tokenStorage = {
  get(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  },
  set(token: string): void {
    sessionStorage.setItem(TOKEN_KEY, token);
  },
  clear(): void {
    sessionStorage.removeItem(TOKEN_KEY);
  }
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly detail?: unknown
  ) {
    super(message);
  }
}

export async function apiRequest<T>(
  baseUrl: string,
  path: string,
  init: RequestInit = {}
): Promise<T> {
  const token = tokenStorage.get();

  const headers = new Headers(init.headers);
  headers.set("Accept", "application/json");

  if (init.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    headers
  });

  if (response.status === 401) {
    tokenStorage.clear();
    window.dispatchEvent(new Event("algocura:unauthorized"));
  }

  if (!response.ok) {
    let detail: unknown;
    try {
      detail = await response.json();
    } catch {
      detail = await response.text();
    }

    throw new ApiError(
      response.status,
      `Request failed with status ${response.status}.`,
      detail
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
