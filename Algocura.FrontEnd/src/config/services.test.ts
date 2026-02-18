import { describe, expect, it } from "vitest";
import { serviceByKey } from "./services";

describe("service operation authorization", () => {
  it("enables reporting CRUD only for roles accepted by the reporting API", () => {
    const reporting = serviceByKey.reporting;
    expect(reporting.crud?.create).toBe(true);
    expect(reporting.crud?.edit).toBe(true);
    expect(reporting.crud?.delete).toBe(true);
    expect(reporting.crud?.editRoles).toEqual(["Scientist", "Reviewer", "Administrator"]);
  });

  it("keeps audit read-only", () => {
    expect(serviceByKey.audit.allowedRoles).toEqual(["Reviewer", "Administrator"]);
    expect(serviceByKey.audit.crud).toBeUndefined();
  });

  it("keeps identity administration restricted to administrators", () => {
    expect(serviceByKey.identity.allowedRoles).toEqual(["Administrator"]);
  });
});
