namespace Korp.Estoque.Application.Features.Product.CreateProduct;

public sealed class CreateProductRequest
{
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int InitialBalance { get; set; }

    public void Normalize()
    {
        Code = Code?.Trim().ToUpperInvariant() ?? string.Empty;
        Description = Description?.Trim() ?? string.Empty;
    }
}
