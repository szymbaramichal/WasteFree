using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Endpoints;

public static class CitiesEndpoints
{
    public static void MapCitiesEndpoints(this WebApplication app)
    {
        app.MapGet("/cities", GetCitiesNames)
            .CacheOutput(c =>
            {
                c.Expire(TimeSpan.FromMinutes(60))
                 .Tag("cities")
                 .SetVaryByHeader(HeaderNames.Origin);
            })
            .WithOpenApi()
            .Produces<Result<string[]>>()
            .WithTags("Cities")
            .WithDescription("Get list of available cities.");
    }
    
    /// <summary>
    /// Get list of available cities in application.
    /// </summary>
    private static Task<IResult> GetCitiesNames(
        IStringLocalizer localizer,
        CancellationToken cancellationToken)
    {
        var cities = new []
        {
            nameof(SupportedCities.Cracow),
            nameof(SupportedCities.Warsaw)
        };
        
        return Task.FromResult(Results.Ok(Result<string[]>.Success(cities)));
    }
}