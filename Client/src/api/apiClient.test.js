import { describe, it, expect } from "vitest";
import apiClient, { normaliseError } from "./apiClient";
import { saveSession } from "../services/sessionService";
import { MESSAGES } from "../constants/messages";

describe("normaliseError", () => {
  it("uses the server's errors array", () => {
    const error = { response: { status: 409, data: { errors: ["Period changed."] } } };

    expect(normaliseError(error)).toEqual({ status: 409, errors: ["Period changed."] });
  });

  it("falls back to a generic message when the body has no errors", () => {
    const error = { response: { status: 500, data: {} } };

    expect(normaliseError(error)).toEqual({ status: 500, errors: [MESSAGES.UNEXPECTED_ERROR] });
  });

  it("reports a network error when there is no response", () => {
    expect(normaliseError(new Error("Network Error"))).toEqual({ status: 0, errors: [MESSAGES.NETWORK_ERROR] });
  });
});

describe("apiClient request interceptor", () => {
  it("adds the bearer token when signed in", async () => {
    saveSession({ token: "jwt-123", user: {}, expiresAtUtc: new Date(Date.now() + 60_000).toISOString() });
    const interceptor = apiClient.interceptors.request.handlers[0].fulfilled;

    const config = await interceptor({ headers: {} });

    expect(config.headers.Authorization).toBe("Bearer jwt-123");
  });

  it("adds nothing when signed out", async () => {
    const interceptor = apiClient.interceptors.request.handlers[0].fulfilled;

    const config = await interceptor({ headers: {} });

    expect(config.headers.Authorization).toBeUndefined();
  });
});
