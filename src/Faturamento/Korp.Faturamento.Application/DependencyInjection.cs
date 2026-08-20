using FluentValidation;
using Korp.Faturamento.Application.Contracts.UseCases;
using Korp.Faturamento.Application.Features.Invoice.CreateInvoice;
using Microsoft.Extensions.DependencyInjection;

namespace Korp.Faturamento.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateInvoiceValidator>();
        services.AddScoped<ICreateInvoiceUseCase, CreateInvoiceHandler>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
