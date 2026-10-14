# Messages: one central place

**Rule:** never type a user-facing sentence inside a controller, service or component. Add it to the messages file and reference the constant.

| Where | File |
|---|---|
| Login, PIN, password reset | `Server/SingularExpress.Api/Constants/Messages/AuthMessages.cs` |
| Financial periods and rollover | `Server/SingularExpress.Api/Constants/Messages/FinancialPeriodMessages.cs` |
| Shared errors (500, 401, 403, 429) | `Server/SingularExpress.Api/Constants/Messages/GeneralMessages.cs` |
| Email subjects and bodies | `Server/SingularExpress.Api/Constants/Messages/EmailTemplates.cs` |
| Browser-side checks | `Client/src/constants/messages.js` |

## Why
- **One place to review wording.** A business analyst can read one file instead of searching 200 files.
- **Consistency.** "Invalid email or password." is spelled the same everywhere. That matters for security, because different wording can leak whether an account exists.
- **Tests stay stable.** Tests assert `AuthMessages.InvalidCredentials`, not a copy of the sentence, so fixing a typo does not break 10 tests.
- **No duplicates.** Before adding a message, search the file. It may already exist.

## How
1. Add a `public const string` to the right class, grouped under a comment for the feature.
2. Use `const`, not `static readonly`. Only constants can be used in attributes such as `[Required(ErrorMessage = AuthMessages.EmailRequired)]`.
3. For values that change, use a format placeholder and `string.Format(CultureInfo.InvariantCulture, ...)`, for example `AccountLockedTooManyAttempts` uses `{0}` for the number of seconds.
4. Add a new class per feature (`LeaveMessages.cs`, `PayrollMessages.cs`) rather than one giant file.

## Is this standard practice?
Yes. Centralising messages is common in .NET teams, in one of two forms:

- **Constants classes** (used here). These are simple, visible and work in attributes. They are best when the app is in one language.
- **Resource files (`.resx`) with `IStringLocalizer`.** This is the built-in .NET approach for translating into several languages. If SingularExpress ever needs Afrikaans, isiZulu or a European language, move these constants into `.resx` files. Each constant becomes a resource key, so the switch is mechanical.

Log messages are **not** included. They are for developers, use structured placeholders (`{UserId}`), and stay next to the code that logs them.
