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

const string AllowLocalFrontendOrigins = "_allowLocalFrontendOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowLocalFrontendOrigins,
        policy  =>
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5000", "https://localhost:5000");
        });
});

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

app.UseCors(AllowLocalFrontendOrigins);

app.UseAuthentication();

app.UseAuthorization();

app.Run();