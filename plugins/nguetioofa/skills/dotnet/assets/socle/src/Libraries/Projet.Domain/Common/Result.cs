namespace Projet.Domain.Common;

/// <summary>L'issue d'une opération : une valeur, ou la raison de son absence.</summary>
/// <remarks>
/// Un service rend ceci plutôt que l'enveloppe HTTP : il reste utilisable depuis une tâche de fond ou un test. Les
/// exceptions restent pour l'imprévu. Les combinateurs évitent la cascade de <c>if (!x.IsSuccess) return x;</c> :
/// une étape qui échoue court-circuite les suivantes.
/// </remarks>
public readonly struct Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        Error = null;
    }

    private Result(Error error)
    {
        _value = default;
        Error = error;
    }

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public bool IsFailure => Error is not null;

    /// <summary>La valeur ; la lire sur un échec est une erreur de programmation.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Résultat en échec : {Error}. Aucune valeur à lire.");

    public T? ValueOrDefault => _value;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    // « return entity.ToDto(); » et « return Error.NotFound(...); » dans la même méthode, sans nommer Result.
    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);

    public Result<TNext> Map<TNext>(Func<T, TNext> map) =>
        IsSuccess ? Result<TNext>.Success(map(_value!)) : Result<TNext>.Failure(Error!);

    public Result<TNext> Bind<TNext>(Func<T, Result<TNext>> bind) =>
        IsSuccess ? bind(_value!) : Result<TNext>.Failure(Error!);

    public Result<T> Ensure(Func<T, bool> predicate, Error error) =>
        IsSuccess && !predicate(_value!) ? Failure(error) : this;

    /// <summary>Échoue si le contrôle rend une erreur ; poursuit s'il rend <c>null</c>.</summary>
    public Result<T> Ensure(Func<T, Error?> check) =>
        IsSuccess && check(_value!) is { } error ? Failure(error) : this;

    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
            action(_value!);

        return this;
    }

    public Result<T> TapError(Action<Error> action)
    {
        if (IsFailure)
            action(Error!);

        return this;
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error!);

    public void Match(Action<T> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
            onSuccess(_value!);
        else
            onFailure(Error!);
    }

    public override string ToString() => IsSuccess ? $"Succès : {_value}" : $"Échec : {Error}";
}

/// <summary>Issue d'une opération sans valeur de retour.</summary>
public readonly struct Result
{
    private Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public bool IsFailure => Error is not null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => new(error);

    public Result<T> With<T>(T value) => IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(Error!);

    public Result Bind(Func<Result> bind) => IsSuccess ? bind() : this;

    public Result Tap(Action action)
    {
        if (IsSuccess)
            action();

        return this;
    }

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error!);

    public override string ToString() => IsSuccess ? "Succès" : $"Échec : {Error}";
}
