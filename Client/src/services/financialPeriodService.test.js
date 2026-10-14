import { describe, it, expect, vi, beforeEach } from "vitest";
import apiClient from "../api/apiClient";
import { getCurrentPeriod, getFinancialYear, rolloverPeriod } from "./financialPeriodService";

vi.mock("../api/apiClient", () => ({ default: { post: vi.fn(), get: vi.fn() } }));

describe("financialPeriodService", () => {
  beforeEach(() => vi.clearAllMocks());

  it("getCurrentPeriod reads the open period", async () => {
    apiClient.get.mockResolvedValue({ data: { id: 7, periodNumber: 7, financialYear: "2026/27" } });

    const period = await getCurrentPeriod();

    expect(apiClient.get).toHaveBeenCalledWith("/financial-periods/current");
    expect(period.periodNumber).toBe(7);
  });

  it("getFinancialYear asks for the year by its start year", async () => {
    apiClient.get.mockResolvedValue({ data: { financialYear: "2026/27", periods: [] } });

    await getFinancialYear(2026);

    expect(apiClient.get).toHaveBeenCalledWith("/financial-periods/years/2026");
  });

  it("rolloverPeriod sends the id of the period being closed", async () => {
    apiClient.post.mockResolvedValue({ data: { message: "Closed" } });

    const result = await rolloverPeriod(7);

    expect(apiClient.post).toHaveBeenCalledWith("/financial-periods/rollover", { expectedCurrentPeriodId: 7 });
    expect(result.message).toBe("Closed");
  });

  it("passes API errors through unchanged", async () => {
    const apiError = { status: 403, errors: ["Only a SuperUser can roll over the financial period."] };
    apiClient.post.mockRejectedValue(apiError);

    await expect(rolloverPeriod(7)).rejects.toBe(apiError);
  });
});
