namespace Projet.Domain.Common;

/// <summary>Les combinateurs de <see cref="Result{T}"/> pour des étapes asynchrones.</summary>
/// <remarks>Sans eux, chaque <c>await</c> couperait la chaîne et ramènerait les <c>if</c>.</remarks>
public static class ResultExtensions
{
    public static async Task<Result<TNext>> MapAsync<T, TNext>(this Task<Result<T>> task, Func<T, TNext> map) =>
        (await task).Map(map);

    public static async Task<Result<TNext>> MapAsync<T, TNext>(this Task<Result<T>> task, Func<T, Task<TNext>> map)
    {
        var result = await task;

        return result.IsSuccess
            ? Result<TNext>.Success(await map(result.Value))
            : Result<TNext>.Failure(result.Error!);
    }

    public static async Task<Result<TNext>> BindAsync<T, TNext>(this Task<Result<T>> task, Func<T, Result<TNext>> bind) =>
        (await task).Bind(bind);

    public static async Task<Result<TNext>> BindAsync<T, TNext>(this Task<Result<T>> task, Func<T, Task<Result<TNext>>> bind)
    {
        var result = await task;

        return result.IsSuccess
            ? await bind(result.Value)
            : Result<TNext>.Failure(result.Error!);
    }

    /// <summary>Une vérification sans valeur, puis ce qu'elle autorise.</summary>
    public static async Task<Result<TNext>> BindAsync<TNext>(this Task<Result> task, Func<Task<Result<TNext>>> bind)
    {
        var result = await task;

        return result.IsSuccess
            ? await bind()
            : Result<TNext>.Failure(result.Error!);
    }

    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> task, Func<T, bool> predicate, Error error) =>
        (await task).Ensure(predicate, error);

    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> task, Func<T, Task<Error?>> check)
    {
        var result = await task;

        if (result.IsFailure)
            return result;

        return await check(result.Value) is { } error ? Result<T>.Failure(error) : result;
    }

    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> task, Action<T> action) =>
        (await task).Tap(action);

    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> task, Func<T, Task> action)
    {
        var result = await task;

        if (result.IsSuccess)
            await action(result.Value);

        return result;
    }

    public static async Task<Result<T>> TapErrorAsync<T>(this Task<Result<T>> task, Action<Error> action) =>
        (await task).TapError(action);

    public static async Task<TOut> MatchAsync<T, TOut>(
        this Task<Result<T>> task, Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        (await task).Match(onSuccess, onFailure);
}
