import { useAuth } from "../../context/AuthContext";
import useFinancialPeriod from "../../hooks/useFinancialPeriod";
import FinancialYearStrip from "../../components/FinancialYearStrip/FinancialYearStrip";
import "../pages.css";

// Landing page after sign-in: the current financial period and the year at a glance.
const HomePage = () => {
  const { user } = useAuth();
  const { current, year, loading, error } = useFinancialPeriod();

  return (
    <main className="page">
      <p className="period-subline">Signed in as {user?.email}</p>

      {loading && <p>Loading the current period…</p>}
      {!loading && error && <div className="alert alert-error" role="alert">{error}</div>}

      {current && (
        <section aria-labelledby="current-period">
          <h1 id="current-period" className="period-headline">
            {current.monthName} {current.calendarYear}
          </h1>
          <p className="period-subline">
            Period {current.periodNumber} of financial year {current.financialYear}
          </p>
        </section>
      )}

      {year && (
        <section className="page-section">
          <h2>Financial year {year.financialYear}</h2>
          <FinancialYearStrip year={year} />
        </section>
      )}
    </main>
  );
};

export default HomePage;
