using Gaia.Operations;
using Microsoft.AspNetCore.Mvc;
using Gaia.Api.Contracts;

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
    public ActionResult<CalculationResponse> ExecuteOperation([FromBody] ExecuteOperationRequest request)
    {
        OperationResult result = _operationExecutor.Execute(request.Operation, request.Left, request.Right);
        if (!result.IsSuccess)
        {
            return BadRequest();
        }
        return new CalculationResponse(result.Value!);
    }




}