using FluentValidation.Results;
using Korp.Estoque.Application.Abstractions.Helpers;
using Korp.Estoque.Application.Contracts.Persistence;
using Korp.Estoque.Application.Contracts.UseCases;
using Korp.Estoque.Domain.Entities.Errors;

namespace Korp.Estoque.Application.Features.Stock.DebitStock;

public sealed class DebitStockHandler(IValidator<DebitStockCommand> validator, IStockDebitRepository stockDebitRepository, TimeProvider timeProvider, ILogger<DebitStockHandler>? logger = null) : IDebitStockUseCase
{
    private readonly ILogger<DebitStockHandler> _logger = logger ?? NullLogger<DebitStockHandler>.Instance;

    #region [ EXECUÇÃO ]

    public async Task<Result<DebitStockResponse>> ExecuteAsync(DebitStockCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando baixa de estoque da nota {InvoiceId}.", command.InvoiceId);
        ValidationResult validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Baixa da nota {InvoiceId} rejeitada por validação.", command.InvoiceId);
            return Result<DebitStockResponse>.Failure(ValidationHelper.ToValidationError(validationResult));
        }

        StockDebitPersistenceCommand persistenceCommand = CreatePersistenceCommand(command);

        StockDebitPersistenceResult persistenceResult = await stockDebitRepository.DebitAsync(persistenceCommand, cancellationToken);

        Result<DebitStockResponse> result = HandlePersistenceResult(persistenceResult);
        if (result.IsSuccess)
            _logger.LogInformation("Baixa da nota {InvoiceId} concluída. Status {DebitStatus}.", command.InvoiceId, persistenceResult.Status);
        else
            _logger.LogWarning("Baixa da nota {InvoiceId} rejeitada. Status {DebitStatus}.", command.InvoiceId, persistenceResult.Status);
        return result;
    }

    #endregion

    #region [ PERSISTÊNCIA ]

    private StockDebitPersistenceCommand CreatePersistenceCommand(DebitStockCommand command)
    {
        return new StockDebitPersistenceCommand
        (
            Guid.NewGuid(),
            command.IdempotencyKey!,
            command.InvoiceId,
            DebitStockPayloadHasher.Compute(command.InvoiceId, command.Items),
            timeProvider.GetUtcNow(),
            command.Items.OrderBy(item => item.ProductId)
                         .Select(item => new StockDebitPersistenceItem(item.ProductId, item.Quantity))
                         .ToList()
        );
    }

    private static Result<DebitStockResponse> HandlePersistenceResult(StockDebitPersistenceResult persistenceResult)
    {
        return persistenceResult.Status switch
        {
            StockDebitPersistenceStatus.Succeeded => Success(persistenceResult, alreadyProcessed: false),
            StockDebitPersistenceStatus.AlreadyProcessed => Success(persistenceResult, alreadyProcessed: true),
            StockDebitPersistenceStatus.ProductNotFound => Result<DebitStockResponse>.Failure(ProductErrors.NotFound),
            StockDebitPersistenceStatus.InsufficientStock => Result<DebitStockResponse>.Failure(ProductErrors.InsufficientStock),
            StockDebitPersistenceStatus.IdempotencyConflict => Result<DebitStockResponse>.Failure(StockOperationErrors.IdempotencyConflict),
            _ => throw new InvalidOperationException($"Resultado de persistência não suportado: {persistenceResult.Status}.")
        };
    }

    #endregion

    #region [ RESPOSTAS ]

    private static Result<DebitStockResponse> Success(StockDebitPersistenceResult persistenceResult, bool alreadyProcessed)
    {
        return Result<DebitStockResponse>.Success
        (
            new DebitStockResponse
            (
                persistenceResult.OperationId,
                persistenceResult.InvoiceId,
                persistenceResult.ProcessedAt,
                alreadyProcessed
            )
        );
    }

    #endregion
}
