using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.Consent
{
    public record GetGarbageAdminConsentQuery(Guid UserId) : IRequest<string>;

    public class GetGarbageGroupDetailsQueryHandler(ApplicationDataContext context) 
        : IRequestHandler<GetGarbageAdminConsentQuery, string>
    {
        public async Task<Result<string>> HandleAsync(GetGarbageAdminConsentQuery request, 
            CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId, 
                cancellationToken: cancellationToken);
            
            if(user is null)
                return Result<string>.Failure(ApiErrorCodes.InvalidUser, HttpStatusCode.NotFound);
            
            var consent = await context.GarbageAdminConsents
                .FirstAsync(x => x.Language == user.LanguagePreference, cancellationToken);

            return Result<string>.Success(consent.Content);
        }
    }
}