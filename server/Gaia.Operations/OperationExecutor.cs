namespace Gaia.Operations;



public sealed class OperationExecutor
{
    private readonly Dictionary<string, IBinaryOperation> _operationsByKey;

    public OperationExecutor(IEnumerable<IBinaryOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        _operationsByKey = operations.ToDictionary(operation => operation.Key, StringComparer.Ordinal);
    }

    public IReadOnlyList<OperationDescriptor> GetAvailableOperations()
    {
        IReadOnlyList<OperationDescriptor> opList =
          (from op in _operationsByKey.Values
           select new OperationDescriptor(op.Key, op.DisplayName))
          .OrderBy(d => d.DisplayName, StringComparer.Ordinal)
          .ThenBy(d => d.Key, StringComparer.Ordinal)
          .ToArray();
        return opList;

    }
    public OperationResult Execute(string key, string left, string right)
    {
        if (!_operationsByKey.TryGetValue(key, out var operation))
        {
            return OperationResult.Failure("unknown_operation", "The requested operation is not available.");
        }
        // A34D: Central dispatch boundary from an operation key to its implementation.

        return operation.Execute(left, right);

    }

}