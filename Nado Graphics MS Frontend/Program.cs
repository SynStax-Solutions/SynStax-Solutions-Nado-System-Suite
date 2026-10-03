using Microsoft.AspNetCore.Authentication.Cookies;
using Nado_Graphics_MS_Frontend.Security;
using Nado_Graphics_MS_Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//This lets the ApiTokenHandler get the logged-in user.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiTokenHandler>();

builder.Services.AddHttpClient("BMSApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]!
    );
})
//This adds the user's token to every API request automatically.
.AddHttpMessageHandler<ApiTokenHandler>();

//---------------- Cookie login ----------------//
//This keeps the user logged in using a cookie.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //These are the pages used for login, logout and access denied.
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        //These are the cookie settings.
        options.Cookie.Name = "NadoGraphics.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = false;
    });

//This turns on role checking ([Authorize]).
builder.Services.AddAuthorization();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrganisationService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<JobCardService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<ProductCategoryService>();
builder.Services.AddScoped<StatementService>();
builder.Services.AddScoped<PortfolioService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

//This checks who the user is, then what they are allowed to do. The order matters.
app.UseAuthentication();
app.UseAuthorization();


//If a page crashes because the API said the user is no longer logged in,
//the user is sent to the login page instead of an error page.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        context.Response.Redirect("/Account/Login");
    }
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();