using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Mockbench.Abstractions.ConfigurationServices;
using Mockbench.Abstractions.Repositories;
using Mockbench.Abstractions.Services;
using Mockbench.Api.Controllers.Admin;
using Mockbench.Components;
using Mockbench.Components.Account;
using Mockbench.Data;
using Mockbench.Data.Contexts;
using Mockbench.Data.Postgres.Contexts;
using Mockbench.Data.PostgresProvider.Services;
using Mockbench.Data.Repositories;
using Mockbench.Data.Services;
using Mockbench.Data.Sqlite.Contexts;
using Mockbench.Data.Sqlite.Services;
using Mockbench.Data.SqlServer.Contexts;
using Mockbench.Data.SqlServer.Services;
using Mockbench.Server.Services;
using Mockbench.Services.MockServices;
using Mockbench.Services.ProxyServices;
using Mockbench.Shared.Models.Configuration;

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


var deploymentConfiguration =
    builder.Configuration.GetSection("DeploymentConfiguration").Get<DeploymentConfiguration>();

builder.Services.Configure<DeploymentConfiguration>(builder.Configuration.GetSection("DeploymentConfiguration"));

switch (deploymentConfiguration.DatabaseConfig.Provider)
{
    case DatabaseProvider.Sqlite:
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString ?? deploymentConfiguration.DatabaseConfig.MainConnectionString, b => b.MigrationsAssembly("Mockbench.Data.Sqlite")));
        builder.Services.AddDbContext<SqliteMockbenchContext>(options => options.UseSqlite(deploymentConfiguration.DatabaseConfig.MainConnectionString, b => b.MigrationsAssembly("Mockbench.Data.Sqlite")));
        builder.Services.AddScoped<MockbenchMainContext, SqliteMockbenchContext>();
        builder.Services.AddScoped<IDatabaseConfigurationService, SqliteDatabaseConfigurationService>();
        break;

    case DatabaseProvider.Postgres:
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(deploymentConfiguration.DatabaseConfig.MainConnectionString, b => b.MigrationsAssembly("Mockbench.Data.Postgres")));
        builder.Services.AddDbContext<SqliteMockbenchContext>(options => options.UseSqlite(deploymentConfiguration.DatabaseConfig.MainConnectionString, b => b.MigrationsAssembly("Mockbench.Data.Postgres")));
        builder.Services.AddScoped<MockbenchMainContext, PostgresMockbenchContext>();
        builder.Services.AddScoped<IDatabaseConfigurationService, PostgresDatabaseConfigurationService>();
        break;
    case DatabaseProvider.SqlServer:
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(deploymentConfiguration.DatabaseConfig.MainConnectionString, b => b.MigrationsAssembly("Mockbench.Data.SqlServer")));
            builder.Services.AddDbContext<SqlServerMockbenchContext>(options => options.UseSqlServer(deploymentConfiguration.DatabaseConfig.MainConnectionString, b => b.MigrationsAssembly("Mockbench.Data.SqlServer")));
            builder.Services.AddScoped<MockbenchMainContext, SqlServerMockbenchContext>();
            builder.Services.AddScoped<IDatabaseConfigurationService, SqlServerDatabaseConfigurationService>();
        }
        break;

    case DatabaseProvider.InMemory:
    default:
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("InMemoryApplicationDb"));
        builder.Services.AddDbContext<MockbenchMainContext>(options => options.UseInMemoryDatabase("InMemoryMockbenchDb"));
        builder.Services.AddScoped<IDatabaseConfigurationService, InMemoryDatabaseConfigurationService>();
        break;
}

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

app.MapControllers();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
