import { ROLES } from "../constants/roles";

// Role checks for showing or hiding UI. The Server enforces the real permission:
// hiding a button is a convenience, not security.

export const isSuperUser = (user) => user?.role === ROLES.SUPER_USER;

export const hasAnyRole = (user, roles) => Boolean(user?.role) && roles.includes(user.role);
