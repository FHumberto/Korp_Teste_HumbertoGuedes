using FluentValidation;
using FluentValidation.Results;
using Korp.Faturamento.Application.Abstractions.Helpers;
using Korp.Faturamento.Application.Abstractions.Wrappers;
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
    TimeProvider timeProvider) : ICreateInvoiceUseCase
{
    public async Task<Result<CreateInvoiceResponse>> ExecuteAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<CreateInvoiceResponse>.Failure(ValidationHelper.ToValidationError(validationResult));

        IReadOnlyCollection<CreateInvoiceItemRequest> items = request.Items!;

        IReadOnlyCollection<InventoryProduct> products;
        try
        {
            products = await inventoryGateway.GetProductsByIdsAsync(
                items.Select(item => item.ProductId).ToArray(), cancellationToken);
        }
        catch (InventoryUnavailableException)
        {
            return Result<CreateInvoiceResponse>.Failure(CreateInvoiceErrors.InventoryUnavailable);
        }

        Dictionary<Guid, InventoryProduct> productsById = products.ToDictionary(product => product.Id);
        if (items.Any(item => !productsById.ContainsKey(item.ProductId)))
            return Result<CreateInvoiceResponse>.Failure(CreateInvoiceErrors.ProductNotFound);

        long number = await invoiceNumberGenerator.GetNextAsync(cancellationToken);
        InvoiceEntity invoice = InvoiceEntity.Create(Guid.NewGuid(), number, timeProvider.GetUtcNow());

        foreach (CreateInvoiceItemRequest item in items)
        {
            InventoryProduct product = productsById[item.ProductId];
            invoice.AddItem(product.Id, product.Code, product.Description, item.Quantity);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);

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
