import { describe, it, expect } from "vitest";
import { isSuperUser, hasAnyRole } from "./roleUtils";

describe("roleUtils", () => {
  it("recognises a SuperUser", () => {
    expect(isSuperUser({ role: "SuperUser" })).toBe(true);
    expect(isSuperUser({ role: "NormalUser" })).toBe(false);
    expect(isSuperUser(null)).toBe(false);
  });

  it("checks a list of roles", () => {
    expect(hasAnyRole({ role: "NormalUser" }, ["SuperUser", "NormalUser"])).toBe(true);
    expect(hasAnyRole({ role: "NormalUser" }, ["SuperUser"])).toBe(false);
    expect(hasAnyRole({}, ["SuperUser"])).toBe(false);
  });
});
