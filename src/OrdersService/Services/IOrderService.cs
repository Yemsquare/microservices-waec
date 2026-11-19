
using OrdersService.DTOs;

namespace OrdersService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken);
}
