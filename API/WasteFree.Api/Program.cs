using System.Reflection;
using FluentValidation;
using TickerQ.DependencyInjection;
using WasteFree.Api.Extensions;
using WasteFree.Api.Middlewares;
using WasteFree.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddSwaggerWithAuth();

builder.Services.RegisterLayers(builder.Configuration)
    .RegisterAuthentication(builder.Configuration)
    .RegisterServices();

builder.Services.AddLocalizationSetup();

builder.Services.AddHealthChecks();

builder.Services
    .AddValidatorsFromAssembly(Assembly.GetCallingAssembly());

builder.Services.AddOutputCache();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.RegisterRateLimiting();

var app = builder.Build();

app.UseSwaggerAndOpenApi(builder.Configuration);

app.UseRequestLocalizationSetup();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MigrateDatabase<ApplicationDataContext>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();
app.MapApplicationHubs();
app.MapHealthChecks("/healthz");

app.UseStaticFiles();

app.UseTickerQ();
app.UseRateLimiter();
app.UseOutputCache();

app.Run();
