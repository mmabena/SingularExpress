using Microsoft.EntityFrameworkCore;
using SingularExpress.Api.Data;
using SingularExpress.Api.Extensions;
using SingularExpress.Api.Middleware;

// ---------------------------------------------------------------------------
// SingularExpress API start-up.
// Keep this file short: registrations live in the Extensions folder, one file per concern.
// ---------------------------------------------------------------------------
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 1. Settings (validated at start-up; secrets come from user-secrets or environment variables)
builder.Services.AddAppOptions(builder.Configuration);

// 2. Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Our services and repositories
builder.Services.AddApplicationServices(builder.Configuration);

// 4. Security: JWT, role policies, rate limiting, CORS
builder.Services.AddJwtAuthentication();
builder.Services.AddAuthRateLimiting(builder.Configuration);
string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy("Client", policy => policy
  .WithOrigins(allowedOrigins)
  .AllowAnyHeader()
  .AllowAnyMethod()));

// 5. Controllers, error handling and Swagger
builder.Services.AddControllers()
  .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
    new System.Text.Json.Serialization.JsonStringEnumConverter()))
  .UseStandardValidationResponse();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerWithJwt();

WebApplication app = builder.Build();

// Start-up data: open financial period (and a dev SuperUser if configured)
using (IServiceScope scope = app.Services.CreateScope())
{
  DbSeeder seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
  await seeder.SeedAsync();
}

// HTTP pipeline (order matters)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}
else
{
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

await app.RunAsync();

/// <summary>Exposed so integration tests can use WebApplicationFactory&lt;Program&gt;.</summary>
public partial class Program
{
}
