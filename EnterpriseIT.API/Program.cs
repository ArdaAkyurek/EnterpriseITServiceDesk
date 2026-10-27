using EnterpriseIT.Application.Interfaces.Repositories;
using EnterpriseIT.Application.Mappings;
using EnterpriseIT.Persistence.Context;
using EnterpriseIT.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using EnterpriseIT.API.Services;
using EnterpriseIT.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.RateLimiting;
using EnterpriseIT.Application.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditActor, HttpAuditActor>();
builder.Services.AddSingleton<TicketAttachmentStorage>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<TokenService>((options, tokens) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = tokens.ValidationParameters;
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var db = context.HttpContext.RequestServices.GetRequiredService<EnterpriseITDbContext>();
                var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = int.TryParse(id, out var userId)
                    ? await db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId)
                    : null;
                if (user == null || !tokens.IsCurrent(context.Principal!, user))
                    context.Fail("Session is no longer valid.");
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser().RequireRole("Admin", "Technician").Build();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("attachment-upload", context => RateLimitPartition.GetFixedWindowLimiter(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
});

builder.Services.AddDbContext<EnterpriseITDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITicketPriorityRepository,TicketPriorityRepository>();
builder.Services.AddScoped<ITicketStatusRepository, TicketStatusRepository>();
builder.Services.AddScoped<ITicketCategoryRepository,TicketCategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IAssetTypeRepository, AssetTypeRepository>();
builder.Services.AddScoped<ITicketCommentRepository,TicketCommentRepository>();
builder.Services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAssetAssignmentRepository, AssetAssignmentRepository>();
builder.Services.AddScoped<IAssetMaintenanceRepository, AssetMaintenanceRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer", BearerFormat = "JWT",
        Description = "Login endpointinden alınan accessToken değerini girin."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Reference = new Microsoft.OpenApi.Models.OpenApiReference
            { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);


var app = builder.Build();

// Explicit, development-only initialization. Never reset an existing password or expose an HTTP setup endpoint.
if (args.Contains("--initialize-admin-password"))
{
    if (!app.Environment.IsDevelopment()) throw new InvalidOperationException("Admin initialization is development-only.");
    var password = Environment.GetEnvironmentVariable("BOOTSTRAP_ADMIN_PASSWORD");
    if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || password.Length > 128)
        throw new InvalidOperationException("Set BOOTSTRAP_ADMIN_PASSWORD to a 12–128 character password.");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EnterpriseITDbContext>();
    var admins = await db.Users.Include(u => u.Role).Where(u => u.Role.RoleName == "Admin" && u.IsActive).ToListAsync();
    if (admins.Count != 1 || !string.IsNullOrEmpty(admins[0].PasswordHash))
        throw new InvalidOperationException("Initialization requires exactly one active Admin with an empty password hash.");
    var admin = admins[0];
    admin.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>().HashPassword(admin, password);
    admin.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    Console.WriteLine($"Admin password initialized: {admin.Email}");
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
