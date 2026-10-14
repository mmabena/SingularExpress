import { describe, it, expect, vi, beforeEach } from "vitest";
import apiClient from "../api/apiClient";
import * as authService from "./authService";
import { getSession } from "./sessionService";

vi.mock("../api/apiClient", () => ({ default: { post: vi.fn(), get: vi.fn() } }));

const EMAIL = "test.user@singular.co.za";

describe("authService", () => {
  beforeEach(() => vi.clearAllMocks());

  it("login posts trimmed credentials and stores the session", async () => {
    const expiresAtUtc = new Date(Date.now() + 3_600_000).toISOString();
    apiClient.post.mockResolvedValue({
      data: { token: "jwt", expiresAtUtc, user: { id: 1, email: EMAIL, role: "SuperUser" } },
    });

    const session = await authService.login(`  ${EMAIL} `, "Secret-1!");

    expect(apiClient.post).toHaveBeenCalledWith("/auth/login", { email: EMAIL, password: "Secret-1!" });
    expect(session.user.role).toBe("SuperUser");
    expect(getSession().token).toBe("jwt");
  });

  it("login does not store anything when the API rejects", async () => {
    apiClient.post.mockRejectedValue({ status: 401, errors: ["Invalid email or password."] });

    await expect(authService.login(EMAIL, "wrong")).rejects.toEqual({
      status: 401,
      errors: ["Invalid email or password."],
    });
    expect(getSession()).toBeNull();
  });

  it("forgotPassword returns the server message", async () => {
    apiClient.post.mockResolvedValue({ data: { message: "sent" } });

    await expect(authService.forgotPassword(EMAIL)).resolves.toBe("sent");
    expect(apiClient.post).toHaveBeenCalledWith("/auth/forgot-password", { email: EMAIL });
  });

  it("verifyPin posts email and PIN", async () => {
    apiClient.post.mockResolvedValue({ data: { message: "ok" } });

    await authService.verifyPin(EMAIL, "123456");

    expect(apiClient.post).toHaveBeenCalledWith("/auth/verify-pin", { email: EMAIL, pin: "123456" });
  });

  it("resetPassword posts all four fields", async () => {
    apiClient.post.mockResolvedValue({ data: { message: "done" } });
    const request = { email: EMAIL, pin: "123456", newPassword: "New-Pass-1!", confirmPassword: "New-Pass-1!" };

    await expect(authService.resetPassword(request)).resolves.toBe("done");
    expect(apiClient.post).toHaveBeenCalledWith("/auth/reset-password", request);
  });

  it("logout clears the session", () => {
    sessionStorage.setItem("singularExpress.session", "{}");
    authService.logout();

    expect(sessionStorage.length).toBe(0);
  });
});
