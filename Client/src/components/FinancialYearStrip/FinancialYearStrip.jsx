import "./FinancialYearStrip.css";

// Shows one financial year as 12 months (April → March) coloured by status:
// Closed (navy), Open (teal outline) and Upcoming (plain).
// `year` is the response of GET /api/financial-periods/years/{financialYearStart}.
const FinancialYearStrip = ({ year }) => {
  if (!year) return null;

  return (
    <figure aria-label={`Financial year ${year.financialYear}`} style={{ margin: 0 }}>
      <ol className="fy-strip">
        {year.periods.map((p) => (
          <li
            key={p.periodNumber}
            className="fy-cell"
            data-status={p.status}
            title={`P${String(p.periodNumber).padStart(2, "0")} ${p.monthName} ${p.calendarYear}: ${p.status}`}
          >
            <span className="fy-cell-month">{p.monthName.slice(0, 3)}</span>
            <span className="fy-cell-number">P{String(p.periodNumber).padStart(2, "0")}</span>
          </li>
        ))}
      </ol>
      <figcaption className="fy-legend">
        <span>Navy: closed</span>
        <span>Outlined: open</span>
        <span>Plain: upcoming</span>
      </figcaption>
    </figure>
  );
};

export default FinancialYearStrip;
