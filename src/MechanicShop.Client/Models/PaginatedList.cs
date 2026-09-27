namespace MechanicShop.Client.Models;

public class PaginatedList<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public List<T> Items { get; set; } = default!;
}