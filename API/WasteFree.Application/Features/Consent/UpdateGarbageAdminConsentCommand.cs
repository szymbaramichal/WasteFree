using System.Net;
using Microsoft.EntityFrameworkCore;
using WasteFree.Application.Abstractions.Messaging;
using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;
using WasteFree.Domain.Models;
using WasteFree.Infrastructure;

namespace WasteFree.Application.Features.Consent
{
    public record UpdateGarbageAdminConsentCommand(string Consent, LanguagePreference Language) : IRequest<string>;

    public class UpdateGarbageAdminConsentCommandHandler(ApplicationDataContext context) 
        : IRequestHandler<UpdateGarbageAdminConsentCommand, string>
    {
        public async Task<Result<string>> HandleAsync(UpdateGarbageAdminConsentCommand request, 
            CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(request.Consent))
                return Result<string>.Failure(ApiErrorCodes.ConsentContentRequired, HttpStatusCode.BadRequest);
            
            var updatedCount = await context
                .GarbageAdminConsents
                .Where(x => x.Language == request.Language)
                .ExecuteUpdateAsync(x => 
                    x.SetProperty(y => y.Content, request.Consent), cancellationToken);

            if (updatedCount > 0)
            {
                await context
                    .Users
                    .Where(x => x.Role == Domain.Enums.UserRole.GarbageAdmin)
                    .ExecuteUpdateAsync(x => 
                        x.SetProperty(y => y.ConsentsAgreed, false), cancellationToken);
                return Result<string>.Success(request.Consent);
            }

            
            return Result<string>.Failure(ApiErrorCodes.ConsentNotFound, HttpStatusCode.NotFound);
        }
    }
}