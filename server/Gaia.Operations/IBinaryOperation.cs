namespace Gaia.Operations;

public interface IBinaryOperation
{
    string Key { get; }
    string DisplayName { get; }
    OperationResult Execute(string left, string right);
}