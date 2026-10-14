# Models

These are EF Core entities, and each one becomes a table. After changing one:
```bash
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```
- Store dates as UTC and name them `...Utc`.
- Store only the personal information a feature needs (POPIA). Add a comment saying why each PI field exists.
- Never return a model from a controller. Map it to a DTO.
