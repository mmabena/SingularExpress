# Data

- `ApplicationDbContext`: tables and indexes.
- `DbSeeder`: start-up data (the open financial period, plus a dev SuperUser from user-secrets).
- `Migrations/`: created by `dotnet ef migrations add`. Commit migrations and never edit an applied one.

Seed only **made-up** data. Never seed real employee information.
