using System.Reflection;
using FluentValidation;
using TickerQ.DependencyInjection;
using WasteFree.Api.Extensions;
using WasteFree.Infrastructure;

const string allowLocalFrontendOrigins = "_allowLocalFrontendOrigins";
 
var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddSwaggerWithAuth();

builder.Services.RegisterLayers(builder.Configuration)
    .RegisterAuthentication(builder.Configuration)
    .RegisterServices();

builder.Services.AddLocalizationSetup();

builder.Services
    .AddValidatorsFromAssembly(Assembly.GetCallingAssembly());

builder.Services.AddOutputCache();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.RegisterCorsPolicy(allowLocalFrontendOrigins);
builder.Services.RegisterRateLimiting();

var app = builder.Build();

app.UseSwaggerAndOpenApi(builder.Configuration);

app.UseRequestLocalizationSetup();

app.MigrateDatabase<ApplicationDataContext>();

app.UseHttpsRedirection();
app.UseCors(allowLocalFrontendOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapApplicationEndpoints();
app.MapApplicationHubs(allowLocalFrontendOrigins);

app.UseStaticFiles();

app.UseTickerQ();
app.UseRateLimiter();
app.UseOutputCache();

app.Run();
