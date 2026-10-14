import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { ROUTES } from "../../constants/routes";
import { MESSAGES } from "../../constants/messages";
import { isAllowedEmail } from "../../utils/validators";
import Wordmark from "./Wordmark";
import "./SignIn.css";

// Sign-in page. Same flow as HRConnect: @singular.co.za email + password,
// show/hide password, link to the forgot-password steps.
const SignIn = () => {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) {
    return <Navigate to={ROUTES.HOME} replace />;
  }

  const validate = () => {
    if (!email.trim()) return MESSAGES.EMAIL_REQUIRED;
    if (!isAllowedEmail(email)) return MESSAGES.EMAIL_DOMAIN;
    if (!password) return MESSAGES.PASSWORD_REQUIRED;
    return "";
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    const problem = validate();
    setError(problem);
    if (problem) return;

    setLoading(true);
    try {
      await login(email, password);
      navigate(location.state?.from ?? ROUTES.HOME, { replace: true });
    } catch (err) {
      setError(err.errors?.[0] ?? MESSAGES.UNEXPECTED_ERROR);
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="signin">
      <section className="signin-form">
        <h1>Welcome!</h1>
        <p className="lead">Sign in with your Singular email address.</p>

        <form onSubmit={handleSubmit} noValidate>
          {location.state?.notice && !error && (
            <div className="alert alert-success" role="status">{location.state.notice}</div>
          )}
          {error && <div className="alert alert-error" role="alert">{error}</div>}

          <div className="field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              placeholder="name@singular.co.za"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              disabled={loading}
            />
          </div>

          <div className="field">
            <label htmlFor="password">Password</label>
            <div className="field-row">
              <input
                id="password"
                type={showPassword ? "text" : "password"}
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={loading}
              />
              <button
                type="button"
                className="button button-secondary"
                onClick={() => setShowPassword((v) => !v)}
                aria-pressed={showPassword}
              >
                {showPassword ? "Hide" : "Show"}
              </button>
            </div>
          </div>

          <div className="signin-actions">
            <button type="submit" className="button button-primary" disabled={loading}>
              {loading ? "Signing in…" : "Sign in"}
            </button>
            <button type="button" className="button-link" onClick={() => navigate(ROUTES.FORGOT_PASSWORD)}>
              Forgot password
            </button>
          </div>
        </form>

        <p className="signin-footer">
          Privacy Policy | Terms &amp; Conditions
          <br />
          Copyright © {new Date().getFullYear()} Singular Systems. All rights reserved.
        </p>
      </section>

      <section className="signin-brand" aria-hidden="true">
        <Wordmark />
      </section>
    </main>
  );
};

export default SignIn;
