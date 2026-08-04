using Gaia.Api.Contracts;
using Gaia.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Gaia.Api.Controllers;

[ApiController]
[Route("api/calculations")]
public sealed class CalculationsController : ControllerBase
{
    private readonly OperationExecutor _operationExecutor;

    public CalculationsController(OperationExecutor operationExecutor)
    {
        _operationExecutor = operationExecutor;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CalculationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult<CalculationResponse> ExecuteOperation(
        [FromBody] ExecuteOperationRequest request
    )
    {
        OperationResult result = _operationExecutor.Execute(
            request.Operation,
            request.Left,
            request.Right
        );
        if (!result.IsSuccess)
        {
            var extensions = new Dictionary<string, object?> { ["errorCode"] = result.ErrorCode };
            return Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Operation failed",
                extensions: extensions
            );
        }
        return new CalculationResponse(result.Value!);
    }
}
