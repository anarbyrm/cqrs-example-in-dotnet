using CqrsExample.Dtos;

namespace CqrsExample.Contracts.Responses;

public class ProductListResponse
{
    public int Size { get; set; }
    public int PageNumber { get; set; }
    public IEnumerable<ProductListDto> Result { get; set; }
}