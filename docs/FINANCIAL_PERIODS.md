# Financial periods

## The year
The financial year runs from **April to March** and is named by both calendar years: **2026/27**. In the database it is stored as `FinancialYearStart = 2026`.

| Period | P01 | P02 | P03 | P04 | P05 | P06 | **P07** | P08 | P09 | P10 | P11 | P12 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Month | Apr 2026 | May | Jun | Jul | Aug | Sep | **Oct 2026** | Nov | Dec 2026 | Jan 2027 | Feb | Mar 2027 |

**Current month: October 2026, which is P07 of 2026/27.** A new database opens this period automatically (`FinancialPeriod:InitialPeriod = "2026-10"`).

All date maths is in `Utils/FinancialYearHelper.cs` and has no database or clock dependency. Use it; never write `month - 3` style calculations anywhere else.

## Rules
- Exactly **one** period is `Open`. The database enforces this with a filtered unique index.
- Closed periods are never reopened.
- A month can exist only once (unique index on year + period).

## Rollover (SuperUser only)
`POST /api/financial-periods/rollover` with `{ "expectedCurrentPeriodId": 7 }`

The checks, in order:
1. **SuperUser?** Checked by the controller policy *and* again in the service.
2. **Is there an open period, and is it the one the Client expected?** If not, the response is 409. This stops a double click or a second SuperUser from skipping a month.
3. **Has the period ended?** Only checked when `AllowRolloverBeforePeriodEnd` is `false`. "Today" is taken in `Africa/Johannesburg` time, not server UTC.
4. **Does the next period already exist?** If so, the response is 409.
5. In **one transaction**, the current period is closed (recording who and when), the next one is opened, and an `AuditLog` row is written. If two rollovers collide, the `RowVersion` and unique indexes make the second one fail with 409. Nothing half-saved remains.

P12 (March) rolls into **P01 (April) of the next financial year**, and `startedNewFinancialYear` is `true`. This is the place to hook year-end work, for example opening leave balances, once those features exist.

## Settings
| Setting | Development | Production |
|---|---|---|
| `AllowRolloverBeforePeriodEnd` | `true`, so learners can test any time | `false` |
| `TimeZoneId` | `Africa/Johannesburg` | `Africa/Johannesburg` |
| `InitialPeriod` | `2026-10` | Set before the first deployment |

## Building features on periods
New features (leave, payroll and others) should store a `FinancialPeriodId` foreign key on their records and read the open period with `IFinancialPeriodService.GetCurrentPeriodAsync()`. Never use `DateTime.Now.Month`.
