# SingularExpress.Tests

These are unit tests (xUnit and Moq). **Every service has a test class.**

| Folder | Tests |
|---|---|
| `Services/` | One `XServiceTests.cs` per service |
| `Utils/` | Pure helper tests |
| `Helpers/` | `TestData` (made-up data only), `TestOptions`, `ListLogger` |

## Conventions
- Name tests `Method_Scenario_ExpectedResult`.
- Use the Arrange / Act / Assert layout, separated by blank lines.
- Mock repositories and external services, and use `FakeTimeProvider` for time.
- Assert on message **constants** (`AuthMessages.InvalidCredentials`), not copied text.
- Never use real names, emails, ID numbers or passwords.

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~FinancialPeriodServiceTests"
```
