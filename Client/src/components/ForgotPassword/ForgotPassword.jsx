import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../../constants/routes";
import Wordmark from "../SignIn/Wordmark";
import RequestResetForm from "./RequestResetForm";
import OtpVerificationForm from "./OtpVerificationForm";
import NewPasswordForm from "./NewPasswordForm";
import "../SignIn/SignIn.css";

// Three steps, same as HRConnect: 1 request a PIN, 2 verify the PIN, 3 choose a new password.
// This parent only remembers which step we are on and the email/PIN between steps.
const STEPS = Object.freeze({ REQUEST: 1, VERIFY: 2, RESET: 3 });

const ForgotPassword = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState(STEPS.REQUEST);
  const [email, setEmail] = useState("");
  const [pin, setPin] = useState("");
  const [notice, setNotice] = useState("");

  const backToSignIn = () => navigate(ROUTES.LOGIN);

  return (
    <main className="signin">
      <section className="signin-form">
        <h1>Reset your password</h1>
        <p className="lead">Step {step} of 3</p>

        {notice && <div className="alert alert-success" role="status">{notice}</div>}

        {step === STEPS.REQUEST && (
          <RequestResetForm
            onPinSent={(sentTo, message) => {
              setEmail(sentTo);
              setNotice(message);
              setStep(STEPS.VERIFY);
            }}
          />
        )}

        {step === STEPS.VERIFY && (
          <OtpVerificationForm
            email={email}
            onVerified={(verifiedPin, message) => {
              setPin(verifiedPin);
              setNotice(message);
              setStep(STEPS.RESET);
            }}
          />
        )}

        {step === STEPS.RESET && (
          <NewPasswordForm
            email={email}
            pin={pin}
            onReset={(message) => navigate(ROUTES.LOGIN, { state: { notice: message } })}
          />
        )}

        <p className="signin-footer">
          <button type="button" className="button-link" onClick={backToSignIn}>
            Back to sign in
          </button>
        </p>
      </section>

      <section className="signin-brand" aria-hidden="true">
        <Wordmark />
      </section>
    </main>
  );
};

export default ForgotPassword;
