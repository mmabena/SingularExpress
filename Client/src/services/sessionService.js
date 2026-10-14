import { SESSION_STORAGE_KEY } from "../constants/config";

// Stores the signed-in session ({ token, user, expiresAtUtc }) in sessionStorage,
// which is cleared when the browser tab closes. Only this file touches storage.

export const saveSession = (session) => {
  sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
};

export const getSession = () => {
  const raw = sessionStorage.getItem(SESSION_STORAGE_KEY);
  if (!raw) return null;
  try {
    const session = JSON.parse(raw);
    if (isExpired(session)) {
      clearSession();
      return null;
    }
    return session;
  } catch {
    clearSession();
    return null;
  }
};

export const getToken = () => getSession()?.token ?? null;

export const clearSession = () => {
  sessionStorage.removeItem(SESSION_STORAGE_KEY);
};

export const isExpired = (session, now = new Date()) => {
  if (!session?.expiresAtUtc) return true;
  return new Date(session.expiresAtUtc).getTime() <= now.getTime();
};
