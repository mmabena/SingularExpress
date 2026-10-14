# Repository

Repositories handle database access with EF Core. They contain queries and saves, **no business rules**.

- Return `null` for "not found" and let the service decide what that means.
- Use `AsNoTracking()` for read-only lists.
- When several writes must succeed or fail together, do them in one method with one transaction (see `FinancialPeriodRepository.CommitRolloverAsync`).
