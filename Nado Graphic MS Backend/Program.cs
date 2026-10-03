using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;
using QuestPDF.Infrastructure;
using Nado_Graphics_MS_Backend.Hubs;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5067",
                "https://localhost:7227")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
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

builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<JobCardService>();
builder.Services.AddScoped<UserService>();

//Service for quote creation and reference counter
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<QuotePDFService>();
builder.Services.Configure<CompanyInfoOptions>(builder.Configuration.GetSection("CompanyInfo"));
builder.Services.AddScoped<ReferenceNumberService>();


builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductCategoryService>();

//These are the new services for login and emails.
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();
//This creates the Owner account if it does not already exist.
//////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BMSDbContext>();

    var ownerEmail = builder.Configuration["SeedOwner:Email"];
    var ownerPassword = builder.Configuration["SeedOwner:Password"];

    if (!string.IsNullOrWhiteSpace(ownerEmail) &&
        !string.IsNullOrWhiteSpace(ownerPassword))
    {
        ownerEmail = ownerEmail.Trim().ToLowerInvariant();

        var ownerExists = await db.Users.AnyAsync(u => u.Email == ownerEmail);

        if (!ownerExists)
        {
            var owner = new User
            {
                FirstName = builder.Configuration["SeedOwner:FirstName"] ?? "Owner",
                LastName = builder.Configuration["SeedOwner:LastName"] ?? "Account",
                Email = ownerEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(ownerPassword),
                UserRole = AppRoles.Manager,
                IsActive = true,
                EmailVerified = true,
                CreatedAt = DateTime.Now
            };

            db.Users.Add(owner);

            await db.SaveChangesAsync();
        }
    }
}
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
//This checks who the user is, then what they are allowed to do. The order matters.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<JobCardHub>("/jobCardHub");
app.Run();