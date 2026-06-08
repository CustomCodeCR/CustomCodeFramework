namespace CustomCodeFramework.Core.Pagination;

public sealed record PagedResult<TItem>
{
    public required IReadOnlyCollection<TItem> Items { get; init; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required long TotalCount { get; init; }

    public long TotalPages => PageSize == 0 ? 0 : (long)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<TItem> Create(
        IReadOnlyCollection<TItem> items,
        int pageNumber,
        int pageSize,
        long totalCount
    )
    {
        return new PagedResult<TItem>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
