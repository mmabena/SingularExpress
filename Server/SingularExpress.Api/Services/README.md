# Services

All business rules live here. Each service:

- implements an interface from `Interfaces/`
- gets dependencies through its constructor (never `new` a repository)
- uses `TimeProvider`, not `DateTime.Now`, so tests can control the clock
- throws an `AppException` subclass with a message from `Constants/Messages`
- **has a matching test class** in `SingularExpress.Tests/Services`

Register new services in `Extensions/ServiceCollectionExtensions.cs`.
