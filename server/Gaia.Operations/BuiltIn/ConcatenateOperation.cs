namespace Gaia.Operations.BuiltIn;

public sealed class ConcatenateOperation : IBinaryOperation
{
    public string Key => "concatenate";
    public string DisplayName => "Concatenate";
    public OperationResult Execute(string left, string right)
    {

        return OperationResult.Success(left + right);


    }

}