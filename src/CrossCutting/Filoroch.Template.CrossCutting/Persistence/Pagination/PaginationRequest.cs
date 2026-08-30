using Microsoft.AspNetCore.Mvc;

namespace Filoroch.Template.CrossCutting.Persistence.Pagination;

public abstract class PaginationRequest
{
    [FromQuery(Name = "qt")]
    public int Quantity { get; set; } = 20;

    [FromQuery(Name = "pg")]
    public int Page { get; set; } = 1;

    [FromQuery(Name = "campoOrdenacao")]
    public string? OrderBy { get; set; }

    [FromQuery(Name = "tipoOrdenacao")]
    public OrderType OrderType { get; set; } = OrderType.Ascending;

    public int Offset => (Page - 1) * Quantity;
}

public enum OrderType
{
    Ascending = 1,
    Descending = 2
}
