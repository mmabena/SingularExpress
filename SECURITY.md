# Security: secrets and personal information

Singular Systems is bound by **POPIA**, and **GDPR** applies to European clients and data. These rules are not optional.

## Secrets
- **Never commit secrets.** This includes the JWT secret, SendGrid API keys, connection strings with passwords and private keys. `appsettings.json` and `appsettings.*.json` (except the example file) are git-ignored.
- Local development uses `dotnet user-secrets`:
  ```bash
  cd Server/SingularExpress.Api
  dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
  dotnet user-secrets set "Email:SendGridApiKey" "<key>"     # only if Email:Provider = SendGrid
  ```
- CI and production use the platform secret store, mapped to environment variables: `Jwt__Secret`, `Email__SendGridApiKey` and `ConnectionStrings__DefaultConnection`.
- The API **refuses to start** if `Jwt:Secret` is missing or shorter than 32 bytes.

## Personal information (PI)
- Store only what a feature needs. For example, `User` holds an email, a password hash and a role, nothing more.
- **Logs:** never log passwords, PINs, tokens or full email addresses. Use `EmailHelper.Mask()`, which turns an address into `t***@singular.co.za`. Log user **ids**, not names.
- **Audit logs** store user ids, not names or emails.
- **Tests and examples** use invented data only (see `SingularExpress.Tests/Helpers/TestData.cs`).
- Do not paste real employee data into tickets, PRs, chat or AI tools.
- Where a feature stores or sends PI, use encryption in transit (HTTPS is on) and at rest (SQL Server TDE in hosted environments), least-privilege database accounts, and a defined retention period. Write the retention rule down in the feature's docs.

## Development email files
With `Email:Provider = File`, emails (including reset PINs) are written to `Server/SingularExpress.Api/App_Data/dev-emails/`. This folder is git-ignored. Use it only with test accounts, and clear it regularly.

## If a secret or PI is exposed
1. Tell **IT Operations** immediately.
2. Rotate the secret (create a new JWT secret or SendGrid key).
3. If it was pushed, purge it from git history (`git filter-repo`) and notify the team.

## Preventative tooling (recommended)
Enable GitHub secret scanning on the repository, and add a pre-commit hook such as `gitleaks`.
