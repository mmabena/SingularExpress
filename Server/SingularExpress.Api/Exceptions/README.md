# Exceptions

These are expected errors, each with its HTTP status:
`BadRequest` 400, `Unauthorized` 401, `Forbidden` 403, `NotFound` 404, `Conflict` 409, `AccountLocked` 423.

Throw them from services, with a message from `Constants/Messages`: `throw new NotFoundException(LeaveMessages.LeaveTypeNotFound);`
