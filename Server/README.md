# Server

This folder holds the .NET back end. Open `SingularExpress.sln` in Visual Studio or Rider, or run `dotnet` commands from here.

| Project | Purpose |
|---|---|
| `SingularExpress.Api` | ASP.NET Core 9 Web API |
| `SingularExpress.Tests` | xUnit and Moq unit tests |

```bash
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage"   # coverage report
```
See the root README for first-time setup.
