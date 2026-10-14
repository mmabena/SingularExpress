// Client-side messages (checks done in the browser before calling the API).
// Messages that come FROM the API are defined on the Server in Constants/Messages
// and shown as-is from response.errors. Keep wording consistent with the Server.
export const MESSAGES = Object.freeze({
  EMAIL_REQUIRED: "Email is required.",
  EMAIL_DOMAIN: "Email must be a @singular.co.za address.",
  PASSWORD_REQUIRED: "Password is required.",
  PIN_REQUIRED: "PIN is required.",
  PIN_FORMAT: "PIN must be 6 digits.",
  NEW_PASSWORD_REQUIRED: "New password is required.",
  PASSWORDS_DO_NOT_MATCH: "Passwords do not match.",
  PASSWORD_COMPLEXITY:
    "Use at least 8 characters with an uppercase letter, a lowercase letter, a digit and a special character.",
  NETWORK_ERROR: "Cannot reach the server. Check that the API is running.",
  UNEXPECTED_ERROR: "Something went wrong. Try again.",
  SESSION_EXPIRED: "Your session has ended. Sign in again.",
  ROLLOVER_IRREVERSIBLE: "Closing a period cannot be undone.",
});
