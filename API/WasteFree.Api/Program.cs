using System.Globalization;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using TickerQ.DependencyInjection;
using WasteFree.Api.Endpoints;
using WasteFree.Api.Extensions;
using WasteFree.Infrastructure;
using WasteFree.Infrastructure.Hubs;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

const string allowLocalFrontendOrigins = "_allowLocalFrontendOrigins";
 
var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Description = "Paste your JWT Bearer token here",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    opt.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
    
    foreach (var xmlFile in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml"))
    {
        try
        {
            opt.IncludeXmlComments(xmlFile);
        }
        catch
        {
            // ignored
        }
    }

    // Add schema filter that injects enum member descriptions (from XML comments) into schema.Description and x-enumDescriptions
    opt.SchemaFilter<WasteFree.Api.Swagger.EnumDescriptionsSchemaFilter>();
});


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

builder.Services.AddOutputCache();
builder.Services.AddSignalR();
 
builder.Services.AddHttpContextAccessor();

builder.Services.RegisterCorsPolicy(allowLocalFrontendOrigins);
builder.Services.RegisterRateLimiting();

var app = builder.Build();

    //if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
//{
    app.UseSwagger();
    
    app.MapOpenApi();
    app.MapScalarApiReference("docs", x =>
    {
        x.Title = $"{builder.Configuration.GetValue<string>("applicationName")} Reference";
        x.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = new[] { "Bearer" }
        };
        x.OpenApiRoutePattern = "/swagger/v1/swagger.json";
        x.WithTheme(ScalarTheme.Kepler);
        x.Servers = new List<ScalarServer>
        {
            new ScalarServer(builder.Configuration["BaseApiUrl"], "DEV server"),
        };
    });
//}

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

app.UseOutputCache();

app.UseTickerQ();

app.MapHub<NotificationHub>("/notificationHub");

app.MapAuthEndpoints();
app.MapGarbageGroupsEndpoints();
app.MapWalletEndpoints();
app.MapAccountEndpoints();
app.MapInboxEndpoints();
app.MapCitiesEndpoints();

app.UseHttpsRedirection();
 
app.UseCors(allowLocalFrontendOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.Run();