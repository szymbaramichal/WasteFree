using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentValidation;
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
 
app.MigrateDatabase<ApplicationDataContext>();
 
app.MapAuthEndpoints();
app.MapGarbageGroupsEndpoints();
 
app.UseHttpsRedirection();
 
app.UseCors(allowLocalFrontendOrigins);

app.UseAuthentication();
 
app.UseAuthorization();

app.UseRateLimiter();

app.Run();