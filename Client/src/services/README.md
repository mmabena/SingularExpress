# services

There is one file per Server area (`authService`, `financialPeriodService`), plus `sessionService` for storage. Each exported function calls one endpoint and returns plain data.

**Every service has a `*.test.js` next to it** that mocks `apiClient`.
