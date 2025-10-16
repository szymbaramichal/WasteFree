using WasteFree.Domain.Entities;
using WasteFree.Domain.Models;

namespace WasteFree.Infrastructure.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, Pager pager)
    {
        return query
            .Skip((pager.PageNumber - 1) * pager.PageSize)
            .Take(pager.PageSize);
    }

    public static IQueryable<GarbageGroup> FilterNonPrivate(this IQueryable<GarbageGroup> query)
    {
        return query.Where(group => !group.IsPrivate);
    }

    public static IQueryable<UserGarbageGroup> FilterNonPrivate(this IQueryable<UserGarbageGroup> query)
    {
        return query.Where(userGroup => !userGroup.GarbageGroup.IsPrivate);
    }
}