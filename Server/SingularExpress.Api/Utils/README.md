# Utils

These are small, **pure** helpers with no database, no clock and no dependency injection, which makes them trivial to test.

| File | Purpose |
|---|---|
| `FinancialYearHelper` | April-March period maths |
| `PasswordValidator` | Complexity rule |
| `EmailHelper` | Normalise, check domain, mask for logs |
| `JwtKeyHelper` | Secret to key bytes, strength check |
