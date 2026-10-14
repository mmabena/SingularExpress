import { useState } from "react";
import { verifyPin } from "../../services/authService";
import { MESSAGES } from "../../constants/messages";
import { RESET_PIN_LENGTH } from "../../constants/config";
import { isValidPin } from "../../utils/validators";

// Step 2: check the 6-digit PIN from the email. It expires after 3 minutes.
const OtpVerificationForm = ({ email, onVerified }) => {
  const [pin, setPin] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!pin) return setError(MESSAGES.PIN_REQUIRED);
    if (!isValidPin(pin)) return setError(MESSAGES.PIN_FORMAT);

    setError("");
    setLoading(true);
    try {
      const message = await verifyPin(email, pin);
      onVerified(pin, message);
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
        <label htmlFor="reset-pin">PIN sent to {email}</label>
        <input
          id="reset-pin"
          inputMode="numeric"
          autoComplete="one-time-code"
          maxLength={RESET_PIN_LENGTH}
          value={pin}
          onChange={(e) => setPin(e.target.value.replace(/\D/g, ""))}
          disabled={loading}
        />
      </div>
      <button type="submit" className="button button-primary" disabled={loading}>
        {loading ? "Checking…" : "Verify PIN"}
      </button>
    </form>
  );
};

export default OtpVerificationForm;
