import { describe, expect, it } from "vitest";
import { parseJwt } from "./jwt";

function token(payload: object): string {
  const encode = (value: object) =>
    btoa(JSON.stringify(value))
      .replace(/\+/g, "-")
      .replace(/\//g, "_")
      .replace(/=+$/g, "");

  return `${encode({ alg: "none" })}.${encode(payload)}.x`;
}

describe("parseJwt", () => {
  it("reads Algocura role claims", () => {
    const parsed = parseJwt(
      token({
        sub: "user-id",
        email: "scientist@algocura.local",
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role":
          "Scientist"
      })
    );

    expect(parsed.roles).toEqual(["Scientist"]);
    expect(parsed.email).toBe(
      "scientist@algocura.local"
    );
  });
});
