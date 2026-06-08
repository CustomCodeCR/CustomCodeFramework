namespace CustomCodeFramework.Postgres.Dapper.Pagination;

public sealed record SqlPagedResult<TItem>
{
    public required IReadOnlyCollection<TItem> Items { get; init; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required long TotalCount { get; init; }

    public long TotalPages => PageSize == 0 ? 0 : (long)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public static SqlPagedResult<TItem> Create(
        IReadOnlyCollection<TItem> items,
        int pageNumber,
        int pageSize,
        long totalCount
    )
    {
        return new SqlPagedResult<TItem>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
        };
    }
}
