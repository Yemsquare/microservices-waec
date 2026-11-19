
using Microsoft.AspNetCore.Mvc;
using OrdersService.Clients;
using OrdersService.DTOs;
using OrdersService.Services;

namespace OrdersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Create),
                new { id = order.Id },
                order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (CatalogUnavailableException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = ex.Message
                });
        }
        catch (CatalogRequestException ex)
        {
            // A failed request to the downstream service should not
            // automatically be reported as a client validation error.
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = "CatalogService returned an unsuccessful response.",
                    remoteStatusCode = (int)ex.StatusCode
                });
        }
    }
}
