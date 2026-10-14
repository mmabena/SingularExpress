# Extensions

These are start-up registrations, one file per concern. They keep `Program.cs` readable.

| File | Registers |
|---|---|
| `OptionsExtensions` | Settings and their validation |
| `ServiceCollectionExtensions` | Services and repositories (**add yours here**) |
| `AuthenticationExtensions` | JWT and role policies |
| `RateLimitingExtensions` | Rate limits on auth endpoints |
| `ApiBehaviourExtensions` | Standard validation error shape |
| `SwaggerExtensions` | Swagger with a JWT button |
| `ClaimsPrincipalExtensions` | `User.GetUserId()` |
