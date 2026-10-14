import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import * as authService from "../services/authService";
import { getSession } from "../services/sessionService";
import { UNAUTHORISED_EVENT } from "../api/apiClient";

// Holds the signed-in user for the whole app. Use it in any component:
//   const { user, isAuthenticated, login, logout } = useAuth();

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [session, setSession] = useState(() => getSession());

  const login = useCallback(async (email, password) => {
    const newSession = await authService.login(email, password);
    setSession(newSession);
    return newSession.user;
  }, []);

  const logout = useCallback(() => {
    authService.logout();
    setSession(null);
  }, []);

  // apiClient fires this event when the Server says the token is no longer valid.
  useEffect(() => {
    const handleUnauthorised = () => setSession(null);
    window.addEventListener(UNAUTHORISED_EVENT, handleUnauthorised);
    return () => window.removeEventListener(UNAUTHORISED_EVENT, handleUnauthorised);
  }, []);

  const value = useMemo(
    () => ({ user: session?.user ?? null, isAuthenticated: Boolean(session), login, logout }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside <AuthProvider>.");
  }
  return context;
};
