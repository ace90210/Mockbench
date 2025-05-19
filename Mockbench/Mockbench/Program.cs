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
using Mockbench.Data.Postgres.Services;
using Mockbench.Data.Repositories;
using Mockbench.Data.Services;
using Mockbench.Data.Sqlite.Services;
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


var deploymentConfiguration = builder.Configuration.GetSection("DeploymentConfiguration").Get<DeploymentConfiguration>();

builder.Services.Configure<DeploymentConfiguration>(builder.Configuration.GetSection("DeploymentConfiguration"));

if(deploymentConfiguration is null)
{
    throw new ArgumentException("DeploymentConfiguration is null");
}

// In development, override with BU (Back Up) connection strings
if (builder.Environment.IsDevelopment())
{
    deploymentConfiguration = SetConnectionStringByProvider(builder, deploymentConfiguration);
}

switch (deploymentConfiguration.DatabaseConfig.Provider)
{
    case DatabaseProvider.SQLite:
        builder.Services.AddSqliteServices(builder.Environment, deploymentConfiguration);
        break;

    case DatabaseProvider.Postgres:
        builder.Services.AddPostgresServices(deploymentConfiguration);
        break;
    case DatabaseProvider.SqlServer:
        {
            builder.Services.AddSqlServerServices(deploymentConfiguration);
        }
        break;

    case DatabaseProvider.InMemory:
    default:
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("InMemoryApplicationDb"));
        builder.Services.AddDbContext<MockbenchDbContext>(options => options.UseInMemoryDatabase("InMemoryMockbenchDb"));
        builder.Services.AddScoped<IDatabaseConfigurationService, InMemoryDatabaseConfigurationService>();
        break;
}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Fix: Specify the name of the HttpClient service explicitly
builder.Services.AddHttpClient("DefaultClient", client =>
{
});


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

app.UseCors(cpb => cpb
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowAnyOrigin());


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


static DeploymentConfiguration SetConnectionStringByProvider(WebApplicationBuilder builder, DeploymentConfiguration deploymentConfiguration)
{
    switch (deploymentConfiguration.DatabaseConfig.Provider)
    {
        case DatabaseProvider.SQLite:
            deploymentConfiguration.DatabaseConfig.MainConnectionString =
                builder.Configuration["BUSqliteDeploymentConfiguration:DatabaseConfig:MainConnectionString"];
            deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString =
                builder.Configuration["BUSqliteDeploymentConfiguration:DatabaseConfig:AuthenticationConnectionString"];
            break;

        case DatabaseProvider.Postgres:
            deploymentConfiguration.DatabaseConfig.MainConnectionString =
                builder.Configuration["BUPostgresDeploymentConfiguration:DatabaseConfig:MainConnectionString"];
            deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString =
                builder.Configuration["BUPostgresDeploymentConfiguration:DatabaseConfig:AuthenticationConnectionString"];
            break;

        case DatabaseProvider.SqlServer:
            deploymentConfiguration.DatabaseConfig.MainConnectionString =
                builder.Configuration["BUSqlServerDeploymentConfiguration:DatabaseConfig:MainConnectionString"];
            deploymentConfiguration.DatabaseConfig.AuthenticationConnectionString =
                builder.Configuration["BUSqlServerDeploymentConfiguration:DatabaseConfig:AuthenticationConnectionString"];
            break;
    }
    return deploymentConfiguration;
}