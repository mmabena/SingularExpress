import { useState } from "react";
import useFinancialPeriod from "../../hooks/useFinancialPeriod";
import { rolloverPeriod } from "../../services/financialPeriodService";
import FinancialYearStrip from "../../components/FinancialYearStrip/FinancialYearStrip";
import ConfirmDialog from "../../components/ConfirmDialog/ConfirmDialog";
import { MESSAGES } from "../../constants/messages";
import "../pages.css";

// SuperUser only (guarded in App.jsx and on the Server).
// Closes the open month and opens the next. March rolls into April of a new financial year.
const nextMonthName = (current) => {
  if (!current) return "";
  const next = new Date(Date.UTC(current.calendarYear, current.calendarMonth, 1));
  return next.toLocaleString("en-ZA", { month: "long", year: "numeric", timeZone: "UTC" });
};

const PeriodManagement = () => {
  const { current, year, loading, error, reload } = useFinancialPeriod();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState("");
  const [rolloverError, setRolloverError] = useState("");

  const target = nextMonthName(current);

  const handleConfirm = async () => {
    setBusy(true);
    setRolloverError("");
    try {
      const response = await rolloverPeriod(current.id);
      setResult(response.message);
      setConfirmOpen(false);
      await reload();
    } catch (err) {
      setRolloverError(err.errors?.[0] ?? MESSAGES.UNEXPECTED_ERROR);
      setConfirmOpen(false);
    } finally {
      setBusy(false);
    }
  };

  return (
    <main className="page">
      <h1 className="period-headline">Period management</h1>

      {loading && <p>Loading…</p>}
      {!loading && error && <div className="alert alert-error" role="alert">{error}</div>}
      {result && <div className="alert alert-success" role="status">{result}</div>}
      {rolloverError && <div className="alert alert-error" role="alert">{rolloverError}</div>}

      {year && (
        <section className="page-section">
          <h2>Financial year {year.financialYear}</h2>
          <FinancialYearStrip year={year} />
        </section>
      )}

      {current && (
        <section className="page-section panel">
          <h2>Month-end rollover</h2>
          <p>
            {current.monthName} {current.calendarYear} (period {current.periodNumber}) is open.
            Rolling over closes it and opens {target}. {MESSAGES.ROLLOVER_IRREVERSIBLE}
          </p>
          <button type="button" className="button button-primary" onClick={() => setConfirmOpen(true)}>
            Roll over to {target}
          </button>
        </section>
      )}

      <ConfirmDialog
        open={confirmOpen}
        title={`Close ${current?.monthName ?? ""} ${current?.calendarYear ?? ""}?`}
        confirmLabel={`Roll over to ${target}`}
        onConfirm={handleConfirm}
        onCancel={() => setConfirmOpen(false)}
        busy={busy}
      >
        <p>
          {current?.label} will be closed and {target} opened. {MESSAGES.ROLLOVER_IRREVERSIBLE}
        </p>
      </ConfirmDialog>
    </main>
  );
};

export default PeriodManagement;
