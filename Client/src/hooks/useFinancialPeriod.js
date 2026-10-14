import { useCallback, useEffect, useState } from "react";
import { getCurrentPeriod, getFinancialYear } from "../services/financialPeriodService";
import { MESSAGES } from "../constants/messages";

// Loads the open period and its whole financial year. Returns { current, year, loading, error, reload }.
// Used by HomePage and PeriodManagement so both stay in sync.
const useFinancialPeriod = () => {
  const [current, setCurrent] = useState(null);
  const [year, setYear] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const reload = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const period = await getCurrentPeriod();
      const overview = await getFinancialYear(period.financialYearStart);
      setCurrent(period);
      setYear(overview);
    } catch (err) {
      setError(err.errors?.[0] ?? MESSAGES.UNEXPECTED_ERROR);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    reload();
  }, [reload]);

  return { current, year, loading, error, reload };
};

export default useFinancialPeriod;
