using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SafranTimeTracker.Api.Extensions;
using SafranTimeTracker.Application.Common.Dtos;
using SafranTimeTracker.Application.Orders.Dtos;
using SafranTimeTracker.Application.Orders.Services;

namespace SafranTimeTracker.Api.Controllers;

/// <summary>Le sous-objet financier d'OrderDto est omis sans FINANCIAL_DATA_VIEW, projection faite
/// par OrderService (CLAUDE.md §13) — même principe que ProjectsController.</summary>
[ApiController]
[Route("api/v1/orders")]
public class OrdersController(
    OrderService service,
    IValidator<OrderCreateRequest> createValidator,
    IValidator<OrderUpdateRequest> updateValidator,
    IValidator<OrderReopenRequest> reopenValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetList(
        [FromQuery] PaginationQuery pagination, [FromQuery] Guid? companyId, [FromQuery] Guid? statusId,
        [FromQuery] Guid? projectId, CancellationToken cancellationToken)
    {
        var result = await service.GetListAsync(pagination, companyId, statusId, projectId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.GetByIdAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, [FromBody] OrderUpdateRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.UpdateAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>§13.2 : machine d'état, transitions dédiées plutôt qu'un champ statut libre.</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<OrderDto>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.ActivateAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<OrderDto>> Suspend(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.SuspendAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/mark-consumed")]
    public async Task<ActionResult<OrderDto>> MarkConsumed(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.MarkConsumedAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<ActionResult<OrderDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var dto = await service.CloseAsync(id, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    /// <summary>§13.4 : seule action capable de sortir une commande de Clôturée, motif obligatoire.</summary>
    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult<OrderDto>> Reopen(Guid id, [FromBody] OrderReopenRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await reopenValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
        }

        var dto = await service.ReopenAsync(id, request, cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }
}
