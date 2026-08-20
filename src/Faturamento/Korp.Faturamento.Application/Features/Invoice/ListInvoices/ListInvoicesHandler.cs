using FluentValidation;
using FluentValidation.Results;
using Korp.Faturamento.Application.Abstractions.Helpers;
using Korp.Faturamento.Application.Abstractions.Wrappers;
using Korp.Faturamento.Application.Contracts.Persistence;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Domain.Enums;

namespace Korp.Faturamento.Application.Features.Invoice.ListInvoices;

public sealed class ListInvoicesHandler(
    IValidator<ListInvoicesRequest> validator,
    IInvoiceRepository invoiceRepository) : IListInvoicesUseCase
{
    public async Task<Result<IReadOnlyCollection<ListInvoicesResponse>>> ExecuteAsync(
        ListInvoicesRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return Result<IReadOnlyCollection<ListInvoicesResponse>>.Failure(ValidationHelper.ToValidationError(validationResult));

        InvoiceStatus? status = string.IsNullOrWhiteSpace(request.Status)
            ? null
            : Enum.Parse<InvoiceStatus>(request.Status, ignoreCase: true);

        IReadOnlyCollection<Domain.Entities.Invoice> invoices = await invoiceRepository.ListAsync(status, cancellationToken);

        return Result<IReadOnlyCollection<ListInvoicesResponse>>.Success(invoices.Select(invoice => new ListInvoicesResponse(
            invoice.Id,
            invoice.Number,
            invoice.Status.ToString().ToLowerInvariant(),
            invoice.Items.Count,
            invoice.CreatedAt,
            invoice.ClosedAt)).ToArray());
    }
}
