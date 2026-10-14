# SingularExpress.Api

The Web API. Each folder has one job:

| Folder | Holds |
|---|---|
| `Controllers/` | HTTP endpoints (thin) |
| `Services/` | Business rules |
| `Interfaces/` | Contracts for services and repositories |
| `Repository/` | Database access |
| `Models/` | EF Core entities (tables) |
| `DTOs/` | Request and response shapes |
| `Mappers/` | Model to DTO conversion |
| `Constants/` | Roles, policies, claim names and **all user-facing messages** |
| `Exceptions/` | `AppException` types mapped to HTTP status codes |
| `Middleware/` | Global error handling |
| `Options/` | Typed, validated settings |
| `Extensions/` | Start-up registration, which keeps `Program.cs` short |
| `Utils/` | Small pure helpers |
| `Data/` | `DbContext`, seeding and migrations |

Settings: copy `appsettings.example.json` to `appsettings.Development.json`, then put secrets in user-secrets (see SECURITY.md).
