using System.Net;
using System.Text.Json.Serialization;

namespace WasteFree.Shared.Models;

/// <summary>
/// Specialized result wrapper that adds pagination metadata. Use this only for endpoints that return paged data
/// so that non-paginated endpoints don't expose the Pager schema in OpenAPI.
/// </summary>
public sealed class PaginatedResult<T> : Result<T>
{
    /// <summary>
    /// Pagination metadata for the current result set.
    /// </summary>
    public Pager Pager { get; init; } = default!;

    private PaginatedResult(T resultModel, Pager pager) : base(resultModel)
    {
        Pager = pager;
    }

    private PaginatedResult(string errorCode, HttpStatusCode responseCode) : base(errorCode, responseCode) { }

    /// <summary>
    /// Create a successful paginated result.
    /// </summary>
    public static PaginatedResult<T> PaginatedSuccess(T value, Pager pager) => new(value, pager);

    /// <summary>
    /// Create a failed paginated result (rarely used – prefer base Result<T>.Failure unless pager must be included).
    /// </summary>
    public static PaginatedResult<T> Failure(string errorCode, HttpStatusCode responseCode = HttpStatusCode.InternalServerError) => new(errorCode, responseCode);
}
