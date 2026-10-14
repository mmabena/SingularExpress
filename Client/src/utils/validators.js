import { ALLOWED_EMAIL_DOMAIN, RESET_PIN_LENGTH } from "../constants/config";

// Browser-side checks give fast feedback. The Server repeats every check;
// never rely on these alone for security.

export const isAllowedEmail = (email) => {
  const value = (email ?? "").trim().toLowerCase();
  return value.length > ALLOWED_EMAIL_DOMAIN.length && value.endsWith(ALLOWED_EMAIL_DOMAIN);
};

// Same rule as Server/SingularExpress.Api/Utils/PasswordValidator.cs
const PASSWORD_RULE = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

export const isStrongPassword = (password) => PASSWORD_RULE.test(password ?? "");

export const isValidPin = (pin) => new RegExp(`^\\d{${RESET_PIN_LENGTH}}$`).test(pin ?? "");
