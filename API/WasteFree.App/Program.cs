using System.Globalization;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using WasteFree.App.Endpoints;
using WasteFree.App.Extensions;
using WasteFree.Infrastructure;

const string allowLocalFrontendOrigins = "_allowLocalFrontendOrigins";
 
var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddOpenApi();
builder.Services.RegisterLayers(builder.Configuration);
builder.Services.RegisterAuthentication(builder.Configuration);
builder.Services.RegisterServices();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<IStringLocalizer>(sp =>
{
    var factory = sp.GetRequiredService<IStringLocalizerFactory>();
    return factory.Create("Shared", typeof(Program).Assembly.FullName!);
});

builder.Services
    .AddValidatorsFromAssembly(Assembly.GetCallingAssembly());
 
builder.Services.AddHttpContextAccessor();

builder.Services.RegisterCorsPolicy(allowLocalFrontendOrigins);
builder.Services.RegisterRateLimiting();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var supportedCultures = new[]
{
    new CultureInfo("pl-PL"),
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(CultureInfo.GetCultureInfo("pl-PL")),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>()
    {
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

 
app.MigrateDatabase<ApplicationDataContext>();
 
app.MapAuthEndpoints();
app.MapGarbageGroupsEndpoints();
 
app.UseHttpsRedirection();
 
app.UseCors(allowLocalFrontendOrigins);

app.UseAuthentication();
 
app.UseAuthorization();

app.UseRateLimiter();

app.Run();