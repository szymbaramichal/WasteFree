using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Exceptions;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ExceptionHandlingMiddleware> logger;
    private readonly IStringLocalizer localizer;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IStringLocalizer localizer)
    {
        this.next = next;
        this.logger = logger;
        this.localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (GeocodingException ex)
        {
            logger.LogWarning(ex, "Geocoding failed for address {Street}, {PostalCode}, {City}",
                ex.Address.Street,
                ex.Address.PostalCode,
                ex.Address.City);

            var result = Result<EmptyResult>.Failure(ApiErrorCodes.GeocodingFailed, HttpStatusCode.BadRequest);
            result.ErrorMessage = localizer[ApiErrorCodes.GeocodingFailed];

            await WriteResultAsync(context, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled application exception");

            var result = Result<EmptyResult>.Failure(ApiErrorCodes.GenericError);
            result.ErrorMessage = localizer[ApiErrorCodes.GenericError];

            await WriteResultAsync(context, result);
        }
    }

    private static Task WriteResultAsync(HttpContext context, Result<EmptyResult> result)
    {
        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)result.ResponseCode;

        return context.Response.WriteAsJsonAsync(result);
    }
}
