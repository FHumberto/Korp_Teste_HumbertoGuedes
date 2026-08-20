namespace Korp.Faturamento.Application.Features.Invoice.ListInvoices;

public sealed class ListInvoicesValidator : AbstractValidator<ListInvoicesRequest>
{
    private static readonly string[] AllowedStatuses = ["open", "closed"];

    public ListInvoicesValidator()
    {
        RuleFor(request => request.Status)
            .Must(status => status is null || AllowedStatuses.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("O status deve ser 'open' ou 'closed'.");
    }
}
