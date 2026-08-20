using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Application.Features.Invoice.CloseInvoice;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Korp.Faturamento.Application.Features.Invoice.GetInvoice;
using Korp.Faturamento.Application.Features.Invoice.ListInvoices;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Faturamento.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateInvoiceValidator>();
        services.AddScoped<ICreateInvoiceUseCase, CreateInvoiceHandler>();
        services.AddScoped<IGetInvoiceUseCase, GetInvoiceHandler>();
        services.AddScoped<IListInvoicesUseCase, ListInvoicesHandler>();
        services.AddScoped<ICloseInvoiceUseCase, CloseInvoiceHandler>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
