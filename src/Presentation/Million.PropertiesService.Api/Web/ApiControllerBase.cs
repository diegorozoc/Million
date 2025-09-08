using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Million.PropertiesService.Api.Web;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult<TResult> OkOrNotFound<TResult>(TResult result) => result is null ? NotFound() : Ok(result);
}
