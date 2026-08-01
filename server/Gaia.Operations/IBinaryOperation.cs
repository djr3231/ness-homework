namespace Gaia.Operations;

public interface IBinaryOperation
{
    string DisplayName { get; }
    string Key { get; }
    OperationResult Execute(string left, string right);
}