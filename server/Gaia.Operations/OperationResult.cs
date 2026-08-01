namespace Gaia.Operations;

public sealed class OperationResult
{

    public bool IsSuccess { get; }
    public string? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private OperationResult(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        IsSuccess = true;
        Value = value;

    }

    private OperationResult(string errorCode, string errorMessage)
    {

        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException(
                "Error code cannot be empty or whitespace.",
                nameof(errorCode));
        }
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "Error message cannot be empty or whitespace.",
                nameof(errorMessage));
        }
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;

    }

    public static OperationResult Success(string value)
    {
        return new OperationResult(value);
    }
    public static OperationResult Failure(string errorCode, string errorMessage)
    {
        return new OperationResult(errorCode, errorMessage);
    }

}