import { Navigate, Outlet, Route, Routes } from "react-router-dom";
import SignIn from "./components/SignIn/SignIn";
import ForgotPassword from "./components/ForgotPassword/ForgotPassword";
import ProtectedRoute from "./components/ProtectedRoute/ProtectedRoute";
import MenuBar from "./components/MenuBar/MenuBar";
import HomePage from "./Pages/HomePage/HomePage";
import PeriodManagement from "./Pages/PeriodManagement/PeriodManagement";
import { ROUTES } from "./constants/routes";
import { ROLES } from "./constants/roles";

// Page layout for signed-in users: menu bar on top, page underneath.
const AppLayout = () => (
  <>
    <MenuBar />
    <Outlet />
  </>
);

// All routes in one place. To add a page: create it in src/Pages, add its path to
// constants/routes.js, then add a <Route> here inside the right guard.
const App = () => (
  <Routes>
    <Route path={ROUTES.LOGIN} element={<SignIn />} />
    <Route path={ROUTES.FORGOT_PASSWORD} element={<ForgotPassword />} />

    <Route element={<ProtectedRoute />}>
      <Route element={<AppLayout />}>
        <Route path={ROUTES.HOME} element={<HomePage />} />

        <Route element={<ProtectedRoute roles={[ROLES.SUPER_USER]} />}>
          <Route path={ROUTES.PERIOD_MANAGEMENT} element={<PeriodManagement />} />
        </Route>
      </Route>
    </Route>

    <Route path="*" element={<Navigate to={ROUTES.HOME} replace />} />
  </Routes>
);

export default App;
