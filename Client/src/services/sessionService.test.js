import { describe, it, expect } from "vitest";
import { saveSession, getSession, getToken, clearSession, isExpired } from "./sessionService";

const future = () => new Date(Date.now() + 60_000).toISOString();
const past = () => new Date(Date.now() - 60_000).toISOString();

describe("sessionService", () => {
  it("saves and reads a session", () => {
    saveSession({ token: "abc", user: { id: 1 }, expiresAtUtc: future() });

    expect(getSession().user.id).toBe(1);
    expect(getToken()).toBe("abc");
  });

  it("returns null and clears storage when the session has expired", () => {
    saveSession({ token: "abc", user: { id: 1 }, expiresAtUtc: past() });

    expect(getSession()).toBeNull();
    expect(sessionStorage.length).toBe(0);
  });

  it("returns null for corrupted storage", () => {
    sessionStorage.setItem("singularExpress.session", "{not json");

    expect(getSession()).toBeNull();
  });

  it("clearSession removes the session", () => {
    saveSession({ token: "abc", user: {}, expiresAtUtc: future() });
    clearSession();

    expect(getToken()).toBeNull();
  });

  it("treats a session without an expiry as expired", () => {
    expect(isExpired({ token: "abc" })).toBe(true);
  });
});
