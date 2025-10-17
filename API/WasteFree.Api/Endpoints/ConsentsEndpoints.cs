using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Application.Features.Consent;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Api.Endpoints;

public static class ConsentsEndpoints
{
	public static void MapConsentsEndpoints(this WebApplication app)
	{
		app.MapGet("/garbage-admin-consents", GetConsentAsync)
			.RequireAuthorization(PolicyNames.GarbageAdminPolicy)
            .CacheOutput(c => c.Expire(TimeSpan.FromMinutes(60)).Tag("consent"))
			.WithOpenApi()
			.Produces<Result<string>>()
			.WithTags("Consents")
			.WithDescription("Get garbage admin consent entry.");

		app.MapPut("/garbage-admin-consents/update", UpdateConsentAsync)
			.RequireAuthorization(PolicyNames.AdminPolicy)
			.WithOpenApi()
			.Produces<Result<string>>()
			.Produces<Result<EmptyResult>>(400)
			.Produces<Result<EmptyResult>>(404)
			.WithTags("Consents")
			.WithDescription("Update garbage admin consent content.");
	}

	/// <summary>
	/// Get garbage admin consent.
	/// </summary>
	private static async Task<IResult> GetConsentAsync(
		ICurrentUserService currentUserService,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new GetGarbageAdminConsentQuery(currentUserService.UserId);

		var result = await mediator.SendAsync(command, cancellationToken);

		return Results.Ok(result);
	}
	
	/// <summary>
	/// Updates garbage admin consent content
	/// </summary>
	private static async Task<IResult> UpdateConsentAsync(
		[FromBody] UpdateConsentRequest request,
		IStringLocalizer localizer,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var command = new UpdateGarbageAdminConsentCommand(request.Consent, request.Language);

		var result = await mediator.SendAsync(command, cancellationToken);

		if (!result.IsValid)
		{
			result.ErrorMessage = localizer[$"{result.ErrorCode}"];
			return Results.Json(result, statusCode: (int)result.ResponseCode);
		}

		return Results.Ok(result);
	}
}



/// <summary>
/// Update garbage admin consent request
/// </summary>
public record UpdateConsentRequest
{
	/// <summary>
	/// Consent content
	/// </summary>
	public string Consent { get; init; } = string.Empty;
	
	/// <summary>
	/// Content language 
	/// </summary>
	public LanguagePreference Language { get; init; }
}
