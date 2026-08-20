using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Korp.Faturamento.Application.Abstractions.Wrappers;

[DebuggerNonUserCode]
[ExcludeFromCodeCoverage]
public static class ResultExtensions
{
    public static TResult Match<TResult>
        (this Result result, Func<TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Error!);
    }

    public static TResult Match<TValue, TResult>(this Result<TValue> result, Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error!);
    }
}
