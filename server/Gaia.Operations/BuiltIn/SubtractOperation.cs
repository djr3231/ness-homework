using System.Globalization;

namespace Gaia.Operations.BuiltIn;

public sealed class SubtractOperation : IBinaryOperation
{
    public string Key => "subtract";
    public string DisplayName => "Subtraction";
    public OperationResult Execute(string left, string right)
    {

        if (!decimal.TryParse(left, CultureInfo.InvariantCulture, out decimal leftDec) ||
        !decimal.TryParse(right, CultureInfo.InvariantCulture, out decimal rightDec)
        )
        {
            return OperationResult.Failure("invalid_numeric_operands", "Both operands must be valid decimal numbers.");

        }
        string difference = (leftDec - rightDec).ToString(CultureInfo.InvariantCulture);
        return OperationResult.Success(difference);


    }

}