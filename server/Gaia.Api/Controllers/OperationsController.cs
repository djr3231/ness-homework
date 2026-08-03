using Gaia.Operations;
using Microsoft.AspNetCore.Mvc;
using Gaia.Api.Contracts;

namespace Gaia.Api.Controllers;


[ApiController]
[Route("api/operations")]
public sealed class OperationsController : ControllerBase
{
    private readonly OperationExecutor _operationExecutor;

    public OperationsController(OperationExecutor operationExecutor)
    {
        _operationExecutor = operationExecutor;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<OperationResponse>> GetAvailableOperations()
    {
        IReadOnlyList<OperationResponse> responses = _operationExecutor.GetAvailableOperations().Select(descriptor => new OperationResponse(descriptor.Key, descriptor.DisplayName)).ToArray();

        return Ok(responses);
    }

}