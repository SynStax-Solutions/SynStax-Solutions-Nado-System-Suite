using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;
using Nado_Graphics_MS_Backend.Repositories;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;
using Nado_Graphics_MS_Backend.Hubs;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters
        .Add(new JsonStringEnumConverter())); ;

builder.Services.AddSignalR();

builder.Services.AddDbContext<BMSDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization"
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

//---------------- JWT authentication ----------------//

//This gets the secret key used to sign tokens. It must be at least 32 characters.
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key in appsettings.json must be at least 32 characters long.");
}

//This makes sure email links point to the real website in production, not to localhost.
var frontendBaseUrl = builder.Configuration["App:FrontendBaseUrl"];
if (!builder.Environment.IsDevelopment() &&
    (string.IsNullOrWhiteSpace(frontendBaseUrl) || frontendBaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException("App:FrontendBaseUrl must be set to the real website address in production.");
}

//This sets up the API to check the JWT on every request.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //This keeps the claim names the same as in the token.
        options.MapInboundClaims = false;

        //These are the checks done on every token.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = AppClaims.Role,
            NameClaimType = AppClaims.Name
        };

        //This runs on every request after the token is checked.
        //It makes sure the user is still allowed in, so deactivating someone works straight away.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                //This gets the database and the user's ID and role from the token.
                var db = context.HttpContext.RequestServices.GetRequiredService<BMSDbContext>();
                var userIdText = context.Principal?.FindFirst(AppClaims.UserId)?.Value;
                var roleInToken = context.Principal?.FindFirst(AppClaims.Role)?.Value;

                if (!int.TryParse(userIdText, out var userId))
                {
                    context.Fail("Invalid token.");
                    return;
                }

                //This gets the user's current details from the database.
                var user = await db.Users
                    .AsNoTracking()
                    .Where(u => u.UserID == userId)
                    .Select(u => new { u.IsActive, u.EmailVerified, u.UserRole, u.PasswordChangedAt })
                    .FirstOrDefaultAsync();

                //This rejects the request if the user was deleted, deactivated, unverified or had their role changed.
                if (user == null || !user.IsActive || !user.EmailVerified || user.UserRole != roleInToken)
                {
                    context.Fail("This account is no longer allowed to log in.");
                    return;
                }

                //This rejects logins made before the password was last changed.
                //(1 second is allowed because the token only stores whole seconds.)
                var tokenIssuedAt = context.SecurityToken.ValidFrom;
                if (user.PasswordChangedAt != null && tokenIssuedAt < user.PasswordChangedAt.Value.AddSeconds(-1))
                {
                    context.Fail("The password was changed. Please log in again.");
                }
            }
        };
    });

//This turns on role checking ([Authorize]).
builder.Services.AddAuthorization();


builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IOrganisationRepository, OrganisationRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IPortfolioSubmissionRepository, PortfolioSubmissionRepository>();
builder.Services.AddScoped<IJobCardRepository, JobCardRepository>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();                                   
builder.Services.AddScoped<IEmailService>(sp => sp.GetRequiredService<EmailService>());
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStatementRepository, StatementRepository>();

builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<OrganisationService>();
builder.Services.AddScoped<JobCardService>();
builder.Services.AddScoped<UserService>();



//Service for quote creation and reference counter
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<QuotePDFService>();
builder.Services.Configure<CompanyInfoOptions>(builder.Configuration.GetSection("CompanyInfo"));
builder.Services.AddScoped<ReferenceNumberService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<InvoicePDFService>();
builder.Services.AddScoped<StatementService>();
builder.Services.AddScoped<StatementPDFService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductCategoryService>();
builder.Services.AddScoped<PortfolioSubmissionService>();

//These are the new services for login and emails.
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<BillingStatusService>();
builder.Services.AddHostedService<BillingStatusWorker>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var frontendOrigins = builder.Configuration.GetSection("Cors:FrontendOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5067", "https://localhost:7227" };

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// One CORS registration with two named policies.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(frontendOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());

    options.AddPolicy("PublicSite", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .WithMethods("POST"));
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    //Strict, login and password endpoints (stops password guessing)
    options.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    //Strict, the public enquiry form (stops spam)
    options.AddPolicy("public-form", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    //Loose, everything else, per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 200, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// Health checks: live = process up, ready = database reachable
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BMSDbContext>("database", tags: new[] { "ready" });

// One error format (RFC 7807 ProblemDetails)
builder.Services.AddProblemDetails();

builder.Services.AddScoped<EnquiryService>();

//This makes the anti-forgery cookie HTTPS-only too.
builder.Services.AddAntiforgery(options => options.Cookie.SecurePolicy = CookieSecurePolicy.Always);

var app = builder.Build();


//---------------- First Owner account ----------------//
//There is no sign-up page, so if there is no Manager in the database yet,
//one is created using the "SeedOwner" details in appsettings.json.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BMSDbContext>();

    var seedEmail = builder.Configuration["SeedOwner:Email"];
    var seedPassword = builder.Configuration["SeedOwner:Password"];

    //This only runs if there is no Manager account yet.
    if (!string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword)
        && !await db.Users.AnyAsync(u => u.UserRole == AppRoles.Manager))
    {
        db.Users.Add(new User
        {
            FirstName = builder.Configuration["SeedOwner:FirstName"] ?? "Owner",
            LastName = builder.Configuration["SeedOwner:LastName"] ?? "Account",
            Email = seedEmail.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword),
            UserRole = AppRoles.Manager,
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.Now
        });
        await db.SaveChangesAsync();

        app.Logger.LogWarning("Created the first Owner/Manager account: {Email}", seedEmail);
    }
}

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseRateLimiter();


//This checks who the user is, then what they are allowed to do. The order matters.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapHub<JobCardHub>("/jobCardHub");

app.Run();