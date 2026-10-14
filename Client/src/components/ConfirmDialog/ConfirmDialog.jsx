import { useEffect, useRef } from "react";
import "./ConfirmDialog.css";

// Accessible confirmation dialog built on the native <dialog> element (focus is trapped,
// Esc cancels). Use it before any irreversible action, e.g. closing a financial period.
const ConfirmDialog = ({ open, title, children, confirmLabel, onConfirm, onCancel, busy = false }) => {
  const ref = useRef(null);

  useEffect(() => {
    const dialog = ref.current;
    if (!open || !dialog || dialog.open) return;
    if (typeof dialog.showModal === "function") {
      dialog.showModal();
    } else {
      dialog.setAttribute("open", ""); // test environments without showModal
    }
  }, [open]);

  if (!open) return null;

  const handleCancel = (event) => {
    event.preventDefault();
    if (!busy) onCancel();
  };

  return (
    <dialog ref={ref} className="confirm-dialog" aria-labelledby="confirm-title" onCancel={handleCancel}>
      <h2 id="confirm-title">{title}</h2>
      <div className="confirm-body">{children}</div>
      <div className="confirm-actions">
        <button type="button" className="button button-secondary" onClick={onCancel} disabled={busy}>
          Cancel
        </button>
        <button type="button" className="button button-primary" onClick={onConfirm} disabled={busy}>
          {busy ? "Working…" : confirmLabel}
        </button>
      </div>
    </dialog>
  );
};

export default ConfirmDialog;
