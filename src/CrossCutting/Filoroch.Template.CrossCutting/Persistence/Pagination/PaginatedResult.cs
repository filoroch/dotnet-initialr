namespace Filoroch.Template.CrossCutting.Persistence.Pagination;

public sealed class PaginatedResult<T>(IReadOnlyList<T> items, int totalItems)
{
    public IReadOnlyList<T> Items { get; } = items;

    public int TotalItems { get; } = totalItems;
}
