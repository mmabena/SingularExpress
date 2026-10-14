# Controllers

These are HTTP endpoints. Keep them **thin**: receive the DTO, call one service method, return the result.

- No try/catch. Services throw `AppException`s and `GlobalExceptionHandler` formats them.
- Put `[Authorize(Policy = Policies.X)]` on the class or the action. The default should be authorised, so only auth endpoints use `[AllowAnonymous]`.
- Add `[ProducesResponseType<T>]` attributes so Swagger documents every response.
- Read the signed-in user's id with `User.GetUserId()`.

**Adding one:** `XController.cs`, route `api/kebab-case-name`, constructor-inject `IXService`.
