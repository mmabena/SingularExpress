import { useState } from "react";
import { forgotPassword } from "../../services/authService";
import { MESSAGES } from "../../constants/messages";
import { isAllowedEmail } from "../../utils/validators";

// Step 1: ask the Server to email a reset PIN.
const RequestResetForm = ({ onPinSent }) => {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!email.trim()) return setError(MESSAGES.EMAIL_REQUIRED);
    if (!isAllowedEmail(email)) return setError(MESSAGES.EMAIL_DOMAIN);

    setError("");
    setLoading(true);
    try {
      const message = await forgotPassword(email);
      onPinSent(email.trim(), message);
    } catch (err) {
      setError(err.errors?.[0] ?? MESSAGES.UNEXPECTED_ERROR);
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate>
      {error && <div className="alert alert-error" role="alert">{error}</div>}
      <div className="field">
        <label htmlFor="reset-email">Email</label>
        <input
          id="reset-email"
          type="email"
          autoComplete="username"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          disabled={loading}
        />
      </div>
      <button type="submit" className="button button-primary" disabled={loading}>
        {loading ? "Sending…" : "Email me a PIN"}
      </button>
    </form>
  );
};

export default RequestResetForm;
