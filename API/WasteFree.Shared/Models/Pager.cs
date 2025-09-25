using System;

namespace WasteFree.Shared.Models;

public class Pager
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
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