import apiClient from "../api/apiClient";

// Calls the Server's /api/financial-periods endpoints.
// Financial year runs April (P01) to March (P12); October 2026 is P07 of 2026/27.

export const getCurrentPeriod = async () => {
  const { data } = await apiClient.get("/financial-periods/current");
  return data;
};

export const getFinancialYear = async (financialYearStart) => {
  const { data } = await apiClient.get(`/financial-periods/years/${financialYearStart}`);
  return data;
};

// SuperUser only. Send the id of the period being closed so a double click,
// or a second SuperUser, can never skip a month.
export const rolloverPeriod = async (expectedCurrentPeriodId) => {
  const { data } = await apiClient.post("/financial-periods/rollover", { expectedCurrentPeriodId });
  return data;
};
