using System.Reflection;
using FluentValidation;
using Scalar.AspNetCore;
using WasteFree.App.Endpoints;
using WasteFree.App.Extensions;
using WasteFree.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.RegisterLayers(builder.Configuration);
builder.Services.RegisterAuthentication(builder.Configuration);
builder.Services.RegisterServices();

builder.Services
    .AddValidatorsFromAssembly(Assembly.GetCallingAssembly());

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.MigrateDatabase<ApplicationDataContext>();

app.MapAuthEndpoints();
app.MapGarbageGroupsEndpoints();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.Run();