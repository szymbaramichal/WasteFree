using WasteFree.Shared.Models;

namespace WasteFree.Infrastructure.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, Pager pager)
    {
        return query
            .Skip((pager.PageNumber - 1) * pager.PageSize)
            .Take(pager.PageSize);
    }
}