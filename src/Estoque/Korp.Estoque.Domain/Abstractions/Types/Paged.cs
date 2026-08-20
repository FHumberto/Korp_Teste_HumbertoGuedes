namespace Korp.Estoque.Domain.Abstractions.Types;

public sealed class Paged<T>(IReadOnlyList<T> items, int totalRecords, int pageNumber, int pageSize)
{
    public IReadOnlyList<T> Items { get; init; } = items;
    public int TotalRecords { get; init; } = totalRecords;
    public int PageNumber { get; init; } = pageNumber;
    public int PageSize { get; init; } = pageSize;

    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
