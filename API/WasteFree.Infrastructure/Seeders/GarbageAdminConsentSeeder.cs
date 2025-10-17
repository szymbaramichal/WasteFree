using Microsoft.EntityFrameworkCore;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Enums;

namespace WasteFree.Infrastructure.Seeders;

public class GarbageAdminConsentSeeder(ApplicationDataContext context)
{
    public async Task SeedAsync()
    {
        var consents = new[]
        {
            new GarbageAdminConsent
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Content = "By confirming this consent, you acknowledge your responsibility for managing waste operations and agree to follow all WasteFree guidelines.",
                Language = LanguagePreference.English,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            },
            new GarbageAdminConsent
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Content = "Potwierdzając tę zgodę, bierzesz odpowiedzialność za zarządzanie gospodarką odpadami i zobowiązujesz się przestrzegać wszystkich wytycznych WasteFree.",
                Language = LanguagePreference.Polish,
                CreatedDateUtc = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            }
        };

        var changesMade = false;

        foreach (var consent in consents)
        {
            if (await context.GarbageAdminConsents.AnyAsync(c => c.Language == consent.Language))
            {
                continue;
            }

            await context.GarbageAdminConsents.AddAsync(consent);
            changesMade = true;
        }

        if (changesMade)
        {
            await context.SaveChangesAsync();
        }
    }
}
