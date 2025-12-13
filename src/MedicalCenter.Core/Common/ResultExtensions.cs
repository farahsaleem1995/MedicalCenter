namespace MedicalCenter.Core.Common;

/// <summary>
/// Extension methods for Result pattern.
/// </summary>
public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value) => Result<T>.Success(value);

    public static Result<T> ToResult<T>(this Error error) => Result<T>.Failure(error);

    public static Result<TResult> Map<T, TResult>(this Result<T> result, Func<T, TResult> func)
    {
        return result.IsSuccess
            ? Result<TResult>.Success(func(result.Value!))
            : Result<TResult>.Failure(result.Error!);
    }

    public static Result<TResult> Bind<T, TResult>(this Result<T> result, Func<T, Result<TResult>> func)
    {
        return result.IsSuccess
            ? func(result.Value!)
            : Result<TResult>.Failure(result.Error!);
    }
}

