using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mockbench.Client.Pages;
using Mockbench.Components;
using Mockbench.Components.Account;
using Mockbench.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();


var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
var connectionStrings = builder.Configuration.GetSection("ConnectionStrings");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (dbProvider.ToLowerInvariant())
    {
        case "sqlite":
            var sqliteConn = connectionStrings.GetValue<string>("Sqlite")
                ?? throw new InvalidOperationException("Missing Sqlite connection string.");
            options.UseSqlite(sqliteConn, sqlite =>
                sqlite.MigrationsAssembly("Mockbench.Data.Sqlite"));
            break;

        case "inmemory":
            options.UseInMemoryDatabase("InMemoryDb");
            break;

        default: // "sqlserver"
            var sqlConn = connectionStrings.GetValue<string>("SqlServer")
                ?? throw new InvalidOperationException("Missing SqlServer connection string.");
            options.UseSqlServer(sqlConn, sql =>
                sql.MigrationsAssembly("Mockbench.Data.SqlServer"));
            break;
    }
});


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Mockbench.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
