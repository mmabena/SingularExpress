# SingularExpress

SingularExpress is a training project for Singular Systems learners. It contains:

- **Server**: ASP.NET Core 9 Web API with SQL Server, EF Core and JWT sign-in.
- **Client**: React 19 with Vite.

The login and security rules come from HRConnect. The financial year runs **April to March**. Moving to the next month (the rollover) is done **manually by a SuperUser** only.

```
SingularExpress/
├── Server/                       Back end (.NET)
│   ├── SingularExpress.sln
│   ├── SingularExpress.Api/      The Web API
│   └── SingularExpress.Tests/    xUnit unit tests for every service
├── Client/                       Front end (React + Vite)
│   └── src/
├── docs/                         How things work (read these first)
├── .github/                      CI pipeline and pull request template
├── CONTRIBUTING.md               Team rules: branches, commits, reviews
└── SECURITY.md                   Secrets and personal information
```

Every folder has its own `README.md` explaining what belongs in it.

## Getting started

### 1. Prerequisites
- .NET 9 SDK
- Node.js 20 or later
- SQL Server (LocalDB or Developer edition)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 2. Server
```bash
cd Server/SingularExpress.Api
cp appsettings.example.json appsettings.Development.json    # git-ignored

# Secrets: never put these in a file (see SECURITY.md)
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
dotnet user-secrets set "Seed:SuperUserEmail" "your.name@singular.co.za"
dotnet user-secrets set "Seed:SuperUserPassword" "<a strong password>"

dotnet ef migrations add InitialCreate      # first time only
dotnet ef database update
dotnet run --launch-profile https           # Swagger: https://localhost:7147/swagger
```

On first start the API opens **October 2026 (period 7 of FY2026/27)**. If the seed settings are present, it also creates your local SuperUser.

### 3. Client
```bash
cd Client
npm install
npm run dev                                 # http://localhost:5173
```

### 4. Tests
```bash
cd Server && dotnet test
cd Client && npm test
```

## Read next
1. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md): how a request flows through the layers.
2. [docs/MESSAGES.md](docs/MESSAGES.md): where every user-facing message lives.
3. [docs/AUTHENTICATION.md](docs/AUTHENTICATION.md): login, lockout and password reset.
4. [docs/FINANCIAL_PERIODS.md](docs/FINANCIAL_PERIODS.md): the April-March year and the rollover.
5. [CONTRIBUTING.md](CONTRIBUTING.md): how 20 people work on one codebase without collisions.
