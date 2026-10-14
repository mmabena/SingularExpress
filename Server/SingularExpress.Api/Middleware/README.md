# Middleware

`GlobalExceptionHandler` turns every exception into `{ errors: [...], traceId }`. Unexpected exceptions return a generic 500. Details are logged, never sent to the Client.
