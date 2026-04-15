using CleanAspireApp.Application.UseCases.Valuations.States;
using CleanAspireApp.Domain.Common.Base;
using CleanAspireApp.Infrastructure;
using CleanAspireApp.Web.Components;
using CleanAspireApp.Web.Extensions;
using CleanAspireApp.Web.Interfaces;
using CleanAspireApp.Web.Services;
using MudBlazor.Services;

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
});

builder.Services.AddScoped<ObjectionState>();
builder.Services.AddScoped<ValuationState>();
builder.Services.AddScoped<ValuationStateMany>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

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


app.Run();
