using System.ComponentModel.DataAnnotations;

namespace Gaia.Api.Contracts;

public sealed class ExecuteOperationRequest
{
    [Required]
    public required string Operation { get; init; }

    [Required(AllowEmptyStrings = true)]
    public required string Left { get; init; }

    [Required(AllowEmptyStrings = true)]
    public required string Right { get; init; }

}