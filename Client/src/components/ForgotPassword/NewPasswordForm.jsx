import { useState } from "react";
import { resetPassword } from "../../services/authService";
import { MESSAGES } from "../../constants/messages";
import { isStrongPassword } from "../../utils/validators";

// Step 3: choose a new password. The Server checks the PIN again, the complexity rule
// and that the password has not been used before.
const NewPasswordForm = ({ email, pin, onReset }) => {
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!newPassword) return setError(MESSAGES.NEW_PASSWORD_REQUIRED);
    if (!isStrongPassword(newPassword)) return setError(MESSAGES.PASSWORD_COMPLEXITY);
    if (newPassword !== confirmPassword) return setError(MESSAGES.PASSWORDS_DO_NOT_MATCH);

    setError("");
    setLoading(true);
    try {
      const message = await resetPassword({ email, pin, newPassword, confirmPassword });
      onReset(message);
    } catch (err) {
      setError(err.errors?.[0] ?? MESSAGES.UNEXPECTED_ERROR);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <p className="lead">{MESSAGES.PASSWORD_COMPLEXITY}</p>
      <div className="field">
        <label htmlFor="new-password">New password</label>
        <input
          id="new-password"
          type="password"
          autoComplete="new-password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          disabled={loading}
        />
      </div>
      <div className="field">
        <label htmlFor="confirm-password">Confirm new password</label>
        <input
          id="confirm-password"
          type="password"
          autoComplete="new-password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          disabled={loading}
        />
      </div>
      <button type="submit" className="button button-primary" disabled={loading}>
        {loading ? "Saving…" : "Save new password"}
      </button>
    </form>
  );
};

export default NewPasswordForm;
