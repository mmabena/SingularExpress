import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { ROUTES } from "../../constants/routes";
import { isSuperUser } from "../../utils/roleUtils";
import Wordmark from "../SignIn/Wordmark";
import "./MenuBar.css";

// Top navigation. SuperUser-only links are hidden for NormalUsers.
const MenuBar = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleSignOut = () => {
    logout();
    navigate(ROUTES.LOGIN, { replace: true });
  };

  return (
    <header className="menubar">
      <Wordmark />
      <nav aria-label="Main">
        <NavLink to={ROUTES.HOME} end>Home</NavLink>
        {isSuperUser(user) && <NavLink to={ROUTES.PERIOD_MANAGEMENT}>Period management</NavLink>}
      </nav>
      <div className="menubar-user">
        <span>{user?.email} ({user?.role})</span>
        <button type="button" className="button button-secondary" onClick={handleSignOut}>
          Sign out
        </button>
      </div>
    </header>
  );
};

export default MenuBar;
