using WasteFree.Domain.Constants;
using WasteFree.Domain.Enums;

namespace WasteFree.Api.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddHierarchicalRolePolicies(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            string userRole = ((int)UserRole.User).ToString();
            string garbageAdminRole = ((int)UserRole.GarbageAdmin).ToString();
            string adminRole = ((int)UserRole.Admin).ToString();

            options.AddPolicy(PolicyNames.UserPolicy, policy =>
            {
                var roles = new List<string> { userRole, adminRole };
                policy.RequireRole(roles);
            });
            
            options.AddPolicy(PolicyNames.GarbageAdminPolicy, policy =>
            {
                var roles = new List<string> { garbageAdminRole, adminRole };
                policy.RequireRole(roles);
            });

            options.AddPolicy(PolicyNames.AdminPolicy, policy =>
            {
                var roles = new List<string> { adminRole };
                policy.RequireRole(roles);
            });

            options.AddPolicy(PolicyNames.GenericPolicy, policy =>
            {
                var roles = new List<string> { userRole, garbageAdminRole, adminRole };
                policy.RequireRole(roles);
            });
        });

        return services;
    }
}
