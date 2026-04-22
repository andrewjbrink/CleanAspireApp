using CleanAspireApp.Application.UseCases.Tenders.States;
using CleanAspireApp.Application.UseCases.Valuations.States;
using CleanAspireApp.Domain.Common.Base;
using CleanAspireApp.Infrastructure;
using CleanAspireApp.Web.Authorization;
using CleanAspireApp.Web.Components;
using CleanAspireApp.Web.Extensions;
using CleanAspireApp.Web.Interfaces;
using CleanAspireApp.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using Polly;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
var apiBaseUrl = builder.Configuration["services:api:https:0"] ?? builder.Configuration["services:api:http:0"]; //services:api:https:0

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true) // Load local overrides
    .AddEnvironmentVariables()
    .Build();

builder.Configuration.AddConfiguration(config);

builder.AddServiceDefaults();
builder.Services.AddWebApiWeb();
builder.AddInfrastructureWeb();
builder.Services.AddScoped<JavaHelper>();
builder.Services.AddScoped<MapService>();
builder.Services.AddScoped<EnqNotifier>();


builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor()
    .AddTransient<AuthorizationHandler>();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<EmailSettings>
    (builder.Configuration.GetSection("EmailSettings"));


builder.Services.AddHttpClient("https+http://apiservice");
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl!)
});

builder.Services.AddHttpClient<ValuationService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
}).AddHttpMessageHandler<AuthorizationHandler>();


builder.Services.AddHttpClient<TenderService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
})
.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(
    retryCount: 3,
    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(60)));



builder.Services.AddScoped<TenderState>();
builder.Services.AddScoped<ObjectionState>();
builder.Services.AddScoped<ValuationState>();
builder.Services.AddScoped<ValuationStateMany>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();



builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.ClientId = builder.Configuration["Auth:ClientId"];
        options.Scope.Add("api://598b5008-a3a7-4a56-a95f-a47716578e7b/lastore_api.all");
        options.RequireHttpsMetadata = true;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SaveTokens = true;
        options.CallbackPath = "/signin-oidc";




        //options.UsePkce = true;
        //options.GetClaimsFromUserInfoEndpoint = true;
        //options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
        //options.ClaimActions.MapUniqueJsonKey("gender", "gender");


        options.Events.OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/authentication-failed");
            return Task.CompletedTask;
        };


        options.Events.OnAuthenticationFailed = context =>
        {
            context.HandleResponse();
            context.Response.Redirect("/authentication-failed");
            return Task.CompletedTask;
        };



        options.Events.OnTokenValidated = context =>
        {
            // You can add custom claims or perform additional validation here if needed
            options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
            options.TokenValidationParameters.RoleClaimType = "roles";

            return Task.CompletedTask;
        };


        options.SignedOutCallbackPath = "/signout-callback-oidc";
        options.RemoteSignOutPath = "/signout-oidc";

    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorizationBuilder();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/config.js", async context =>
{
    context.Response.ContentType = "application/javascript";
    await context.Response.WriteAsync($"window.__config = {{ apiBaseUrl: '{apiBaseUrl}' }};");
});

app.MapLoginLogout();

app.Run();
