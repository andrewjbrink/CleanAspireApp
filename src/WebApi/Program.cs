using CleanAspireApp.Application;
using CleanAspireApp.Infrastructure;
using CleanAspireApp.WebApi;
using CleanAspireApp.WebApi.Endpoints;
using CleanAspireApp.WebApi.Extensions;
using CleanAspireApp.WebApi.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCustomProblemDetails();

builder.Services.AddWebApi(builder.Configuration);
builder.Services.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapOpenApi();
app.MapCustomScalarApiReference();
app.UseHealthChecks();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapHeroEndpoints();
app.MapTeamEndpoints();
app.MapValuationEndpoints();
app.UseEventualConsistencyMiddleware();

app.ApplyApiCorsConfig();
app.MapDefaultEndpoints();
app.UseExceptionHandler();

app.Run();