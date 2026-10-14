# Contributing

There are 20 people working on this codebase. These rules keep it consistent and reviewable.

## Branches
- `main` is protected. Nobody pushes to it directly.
- Create one branch per ticket: `feature/SE-123-short-description` or `fix/SE-123-short-description`. This is the same pattern as HRConnect's `feature/HRC008`.
- Keep branches short-lived. Rebase on `main` at least daily.

## Pull requests
- Fill in the PR template.
- Each PR needs **one approving review** from someone who did not write it.
- CI must pass: build, all Server tests and all Client tests.
- Keep PRs under about 400 changed lines. Split anything bigger.

## Definition of done
- [ ] Every new or changed **service** has unit tests (Server: `SingularExpress.Tests/Services`; Client: a `*.test.js` file next to the service).
- [ ] Every user-facing message is in `Constants/Messages` (Server) or `constants/messages.js` (Client). See docs/MESSAGES.md.
- [ ] No secrets, real personal information or credentials in code, tests, logs or commits. See SECURITY.md.
- [ ] New settings are added to `appsettings.example.json` with a safe placeholder.
- [ ] The folder README is updated if you added a new kind of file.

## Server conventions
- Request flow: **Controller → Service → Repository → DbContext**. Never skip a layer.
- Controllers are thin and contain no try/catch. Services throw an `AppException` subclass, and `GlobalExceptionHandler` formats it.
- Use explicit types, not `var` (enforced by `.editorconfig`, copied from HRConnect).
- Use 2-space indentation and file-scoped namespaces. Keep usings outside the namespace.
- Name tests `Method_Scenario_ExpectedResult` and use only made-up data (`TestData`).

## Client conventions
- Components never call axios directly. They call a function in `src/services`.
- Pages go in `src/Pages`, reusable pieces in `src/components`, and URLs in `constants/routes.js`.
- Use the CSS variables in `styles/global.css`, never hard-coded colours.

## Commit messages
`SE-123: Add leave balance service` uses imperative mood and the ticket number first.
