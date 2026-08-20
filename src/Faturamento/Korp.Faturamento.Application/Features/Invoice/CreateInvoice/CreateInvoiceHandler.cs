using FluentValidation.Results;
using Korp.Faturamento.Application.Abstractions.Helpers;
using Korp.Faturamento.Application.Contracts.Gateways;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Contracts.UseCases;
using InvoiceEntity = Korp.Faturamento.Domain.Entities.Invoice;

namespace Korp.Faturamento.Application.Features.Invoice.CreateInvoice;

public sealed class CreateInvoiceHandler(
    IValidator<CreateInvoiceRequest> validator,
    IInventoryGateway inventoryGateway,
    IInvoiceNumberGenerator invoiceNumberGenerator,
    IInvoiceRepository invoiceRepository,
    TimeProvider timeProvider,
    ILogger<CreateInvoiceHandler>? logger = null) : ICreateInvoiceUseCase
{
    private readonly ILogger<CreateInvoiceHandler> _logger = logger ?? NullLogger<CreateInvoiceHandler>.Instance;

    public async Task<Result<CreateInvoiceResponse>> ExecuteAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Iniciando criação de nota com {ItemCount} itens.", request.Items?.Count ?? 0);
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Criação de nota rejeitada por validação.");
            return Result<CreateInvoiceResponse>.Failure(ValidationHelper.ToValidationError(validationResult));
        }

        IReadOnlyCollection<CreateInvoiceItemRequest> items = request.Items!;

        IReadOnlyCollection<InventoryProduct> products;
        try
        {
            products = await inventoryGateway.GetProductsByIdsAsync(
                items.Select(item => item.ProductId).ToArray(), cancellationToken);
        }
        catch (InventoryUnavailableException exception)
        {
            _logger.LogError(exception, "Estoque indisponível durante a criação da nota.");
            return Result<CreateInvoiceResponse>.Failure(CreateInvoiceErrors.InventoryUnavailable);
        }

        Dictionary<Guid, InventoryProduct> productsById = products.ToDictionary(product => product.Id);
        if (items.Any(item => !productsById.ContainsKey(item.ProductId)))
        {
            _logger.LogWarning("Criação de nota rejeitada porque um ou mais produtos não foram encontrados.");
            return Result<CreateInvoiceResponse>.Failure(CreateInvoiceErrors.ProductNotFound);
        }

        long number = await invoiceNumberGenerator.GetNextAsync(cancellationToken);
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), number, timeProvider.GetUtcNow());

        foreach (CreateInvoiceItemRequest item in items)
        {
            InventoryProduct product = productsById[item.ProductId];
            invoice.AddItem(product.Id, product.Code, product.Description, item.Quantity);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);

        _logger.LogInformation("Nota {InvoiceId}, número {InvoiceNumber}, criada com sucesso.", invoice.Id, invoice.Number);
        return Result<CreateInvoiceResponse>.Success(new CreateInvoiceResponse(
            invoice.Id,
            invoice.Number,
            invoice.Status.ToString().ToLowerInvariant(),
            invoice.Items.Select(item => new CreateInvoiceItemResponse(
                item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity)).ToArray(),
            invoice.CreatedAt,
            invoice.ClosedAt));
    }
}
