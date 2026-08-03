namespace Gaia.Operations.BuiltIn;

public sealed class ContainsOperation : IBinaryOperation
{
    public string Key => "contains";
    public string DisplayName => "Contains";
    public OperationResult Execute(string left, string right)
    {
        bool result = left.Contains(right, StringComparison.Ordinal);
        return OperationResult.Success(result ? "true" : "false");
    }
}