namespace CustomCodeFramework.Core.Pagination;

public sealed record PageRequest
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 200;

    public int PageNumber { get; init; } = DefaultPageNumber;

    public int PageSize { get; init; } = DefaultPageSize;

    public int Skip => (PageNumber - 1) * PageSize;

    public static PageRequest Create(int pageNumber, int pageSize)
    {
        pageNumber = pageNumber <= 0 ? DefaultPageNumber : pageNumber;
        pageSize = pageSize <= 0 ? DefaultPageSize : pageSize;
        pageSize = Math.Min(pageSize, MaxPageSize);

        return new PageRequest { PageNumber = pageNumber, PageSize = pageSize };
    }
}
