namespace Korp.Estoque.Domain.Abstractions.Types;

public sealed class Paged<T>(IReadOnlyList<T> items, int totalRecords, int pageNumber, int pageSize)
{
    public IReadOnlyList<T> Items { get; set; } = items;
    public int TotalRecords { get; set; } = totalRecords;
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
