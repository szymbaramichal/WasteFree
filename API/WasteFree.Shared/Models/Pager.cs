using System;

namespace WasteFree.Shared.Models;

/// <summary>
/// Represents paging metadata for paginated responses.
/// Use this object to convey the current page, page size and total counts so clients can render pagination controls.
/// </summary>
public class Pager
{
    /// <summary>
    /// Current page number (1-based). When constructed, values less than 1 are normalized to 1.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page. When constructed, values less than 1 are normalized to 10.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items available across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages calculated as Ceiling(TotalCount / (double)PageSize).
    /// Note: PageSize should be greater than zero; when created via the provided constructors PageSize is normalized to at least 1.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);


    public Pager(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
    }
    

    public Pager(int pageNumber, int pageSize, int totalCount) : this(pageNumber, pageSize)
    {
        TotalCount = totalCount < 0 ? 0 : totalCount;
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
    }
}