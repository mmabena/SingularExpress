import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import * as periodService from "../../services/financialPeriodService";
import PeriodManagement from "./PeriodManagement";

vi.mock("../../services/financialPeriodService", () => ({
  getCurrentPeriod: vi.fn(),
  getFinancialYear: vi.fn(),
  rolloverPeriod: vi.fn(),
}));

const october = {
  id: 7,
  financialYear: "2026/27",
  financialYearStart: 2026,
  periodNumber: 7,
  calendarYear: 2026,
  calendarMonth: 10,
  monthName: "October",
  label: "2026/27 P07 (October 2026)",
};

const months = ["April", "May", "June", "July", "August", "September", "October", "November", "December", "January", "February", "March"];
const year = {
  financialYear: "2026/27",
  financialYearStart: 2026,
  periods: months.map((m, i) => ({
    periodNumber: i + 1,
    monthName: m,
    calendarYear: i < 9 ? 2026 : 2027,
    status: i < 6 ? "Closed" : i === 6 ? "Open" : "Upcoming",
  })),
};

describe("PeriodManagement", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    periodService.getCurrentPeriod.mockResolvedValue(october);
    periodService.getFinancialYear.mockResolvedValue(year);
  });

  it("asks for confirmation and sends the open period's id", async () => {
    periodService.rolloverPeriod.mockResolvedValue({ message: "Closed October. November is now open." });
    render(<PeriodManagement />);

    await userEvent.click(await screen.findByRole("button", { name: "Roll over to November 2026" }));
    expect(periodService.rolloverPeriod).not.toHaveBeenCalled();

    const dialog = screen.getByRole("dialog");
    await userEvent.click(dialog.querySelector(".button-primary"));

    expect(periodService.rolloverPeriod).toHaveBeenCalledWith(7);
    expect(await screen.findByRole("status")).toHaveTextContent("November is now open.");
  });

  it("does nothing when the user cancels", async () => {
    render(<PeriodManagement />);

    await userEvent.click(await screen.findByRole("button", { name: "Roll over to November 2026" }));
    await userEvent.click(screen.getByRole("button", { name: "Cancel" }));

    expect(periodService.rolloverPeriod).not.toHaveBeenCalled();
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("shows the server's message when rollover is refused", async () => {
    periodService.rolloverPeriod.mockRejectedValue({ status: 409, errors: ["The open period has changed."] });
    render(<PeriodManagement />);

    await userEvent.click(await screen.findByRole("button", { name: "Roll over to November 2026" }));
    await userEvent.click(screen.getByRole("dialog").querySelector(".button-primary"));

    expect(await screen.findByRole("alert")).toHaveTextContent("The open period has changed.");
  });
});
