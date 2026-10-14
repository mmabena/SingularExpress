import axios from "axios";
import { getToken, clearSession } from "../services/sessionService";
import { MESSAGES } from "../constants/messages";

// The one axios instance the whole Client uses.
// - Adds "Authorization: Bearer <token>" to every request.
// - Turns every failure into { status, errors: [...] } so components only handle one shape.

export const UNAUTHORISED_EVENT = "singularExpress:unauthorised";

const apiClient = axios.create({
  baseURL: `${import.meta.env.VITE_API_URL ?? ""}/api`,
  headers: { "Content-Type": "application/json" },
});

apiClient.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// The Server always answers errors with { errors: [...] } (see GlobalExceptionHandler).
export const normaliseError = (error) => {
  if (!error?.response) {
    return { status: 0, errors: [MESSAGES.NETWORK_ERROR] };
  }
  const { status, data } = error.response;
  const errors = Array.isArray(data?.errors) && data.errors.length > 0
    ? data.errors
    : [MESSAGES.UNEXPECTED_ERROR];
  return { status, errors };
};

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const normalised = normaliseError(error);
    const isAuthCall = error?.config?.url?.startsWith("/auth/");
    if (normalised.status === 401 && !isAuthCall) {
      clearSession();
      window.dispatchEvent(new Event(UNAUTHORISED_EVENT));
    }
    return Promise.reject(normalised);
  },
);

export default apiClient;
