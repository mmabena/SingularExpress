import apiClient from "../api/apiClient";
import { saveSession, clearSession } from "./sessionService";

// Calls the Server's /api/auth endpoints. Same steps as HRConnect:
// login → forgot password (email PIN) → verify PIN → reset password.
// Every function throws { status, errors } on failure (see apiClient.normaliseError).

export const login = async (email, password) => {
  const { data } = await apiClient.post("/auth/login", { email: email.trim(), password });
  const session = { token: data.token, user: data.user, expiresAtUtc: data.expiresAtUtc };
  saveSession(session);
  return session;
};

export const forgotPassword = async (email) => {
  const { data } = await apiClient.post("/auth/forgot-password", { email: email.trim() });
  return data.message;
};

export const verifyPin = async (email, pin) => {
  const { data } = await apiClient.post("/auth/verify-pin", { email: email.trim(), pin });
  return data.message;
};

export const resetPassword = async ({ email, pin, newPassword, confirmPassword }) => {
  const { data } = await apiClient.post("/auth/reset-password", {
    email: email.trim(),
    pin,
    newPassword,
    confirmPassword,
  });
  return data.message;
};

export const logout = () => {
  clearSession();
};
