# DTOs

Data Transfer Objects define what goes over HTTP. There is one subfolder per feature.

- Validate input with attributes whose messages come from `Constants/Messages`: `[Required(ErrorMessage = AuthMessages.EmailRequired)]`.
- Request DTOs end in `...RequestDto` or `...Dto`. Response DTOs describe exactly what the Client needs and nothing more.
