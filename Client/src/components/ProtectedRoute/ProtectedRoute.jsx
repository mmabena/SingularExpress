import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { ROUTES } from "../../constants/routes";
import { hasAnyRole } from "../../utils/roleUtils";

// Wrap routes that need a signed-in user. Pass roles to limit a route further:
//   <Route element={<ProtectedRoute roles={[ROLES.SUPER_USER]} />}> ... </Route>
// This only hides pages; the Server still checks the role on every request.
const ProtectedRoute = ({ roles }) => {
  const { user, isAuthenticated } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to={ROUTES.LOGIN} replace state={{ from: location.pathname }} />;
  }

  if (roles && !hasAnyRole(user, roles)) {
    return <Navigate to={ROUTES.HOME} replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
