using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Korp.Faturamento.Domain.Abstractions.Types;

namespace Korp.Faturamento.Application.Abstractions.Wrappers;

[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
public class Result<T>
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    protected Result(Error error)
    {
        IsSuccess = false;
        Error = error;
        Value = default;
    }

    [AllowNull]
    public T Value => IsSuccess ? field! : throw new InvalidOperationException();

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);
}

public sealed class Result : Result<Unit>
{
    private Result(Unit value) : base(value) { }
    private Result(Error error) : base(error) { }
    public static Result Success() => new(Unit.Value);
    public static new Result Failure(Error error) => new(error);
    public static implicit operator Result(Error error) => new(error);
}
