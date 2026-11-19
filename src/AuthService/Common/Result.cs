namespace AuthService.Common;

public enum ErrorType { None, Validation, Unauthorized, NotFound }

public sealed class Result<T>
{
    public T? Value { get; private init; }
    public ErrorType Error { get; private init; }
    public string? Message { get; private init; }
    public IDictionary<string, string[]>? Errors { get; private init; }
    public bool Succeeded => Error == ErrorType.None;

    public static Result<T> Ok(T value) => new() { Value = value };
    public static Result<T> Fail(ErrorType error, string? message = null,
        IDictionary<string, string[]>? errors = null) =>
        new() { Error = error, Message = message, Errors = errors };
}