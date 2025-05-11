using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Abstractions.Repositories;
using Mockbench.Abstractions.Services;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Client.Pages;
using Mockbench.Components;
using Mockbench.Components.Account;
using Mockbench.Data;
using Mockbench.Data.Contexts;
using Mockbench.Data.Repositories;
using Mockbench.Data.Services;
using Mockbench.Data.Sqlite.Services;
using Mockbench.Data.SqlServer.Services;
using Mockbench.Server.Services;
using Mockbench.Services.MockServices;
using Mockbench.Services.ProxyServices;

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

builder.Services.AddScoped<IBaseRepository, BaseRepository>();
builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IEnvironmentRepository, EnvironmentRepository>();
builder.Services.AddScoped<IMicroserviceRepository, MicroserviceRepository>();
builder.Services.AddScoped<IEndpointRepository, EndpointRepository>();
builder.Services.AddScoped<IMockResponseRepository, MockResponseRepository>();

builder.Services.AddScoped<IMockService, MockService>();
builder.Services.AddScoped<ISimulateTimeService, SimulateTimeService>();
builder.Services.AddScoped<IProxyService, ProxyService>();
builder.Services.AddScoped<IHttpService, HttpService>();

builder.Services.AddMvc()
    .AddApplicationPart(typeof(ConfigurationController).Assembly)
    .AddControllersAsServices();

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

builder.Services.AddDbContext<MockbenchMainContext>(options =>
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

switch (dbProvider.ToLowerInvariant())
{
    case "sqlite":
        builder.Services.AddScoped<IDatabaseConfigurationService, SqliteDatabaseConfigurationService>();
        break;

    case "inmemory":
        builder.Services.AddScoped<IDatabaseConfigurationService, InMemoryDatabaseConfigurationService>();
        break;

    default:
        builder.Services.AddScoped<IDatabaseConfigurationService, SqlServerDatabaseConfigurationService>();
        break;
}

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

app.MapControllers();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
