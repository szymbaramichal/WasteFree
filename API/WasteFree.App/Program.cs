using Scalar.AspNetCore;
using WasteFree.App.Endpoints;
using WasteFree.App.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.RegisterLayers(builder.Configuration);

var app = builder.Build();

app.MapAuthEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();


app.Run();