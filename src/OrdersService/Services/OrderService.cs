
using OrdersService.Clients;
using OrdersService.DTOs;
using OrdersService.Models;

namespace OrdersService.Services;

public sealed class OrderService : IOrderService
{
    private readonly ICatalogClient _catalogClient;
    private readonly ILogger<OrderService> _logger;

    // Temporary in-memory storage
    private static readonly List<ExaminationOrder> Orders = [];

    public OrderService(
        ICatalogClient catalogClient,
        ILogger<OrderService> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Step 1: Remove duplicate subject IDs.
        var requestedIds = request.SubjectIds
            .Distinct()
            .ToList();

        // Step 2: Ask CatalogService to verify the subjects.
        var subjects = await _catalogClient.GetSubjectsAsync(
            requestedIds,
            cancellationToken);

        // Step 3: Identify missing subjects.
        var returnedIds = subjects
            .Select(subject => subject.Id)
            .ToHashSet();

        var missingIds = requestedIds
            .Where(id => !returnedIds.Contains(id))
            .ToList();

        if (missingIds.Count > 0)
        {
            throw new ArgumentException(
                $"The following subject IDs do not exist: " +
                $"{string.Join(", ", missingIds)}");
        }

        // Step 4: Identify inactive subjects.
        var inactiveSubjects = subjects
            .Where(subject => !subject.IsActive)
            .Select(subject => subject.Id)
            .ToList();

        if (inactiveSubjects.Count > 0)
        {
            throw new ArgumentException(
                $"The following subjects are inactive: " +
                $"{string.Join(", ", inactiveSubjects)}");
        }

        // Step 5: Create the order only after validation succeeds.
        var order = new ExaminationOrder
        {
            CandidateNumber = request.CandidateNumber,
            SubjectIds = requestedIds,
            Status = "Created"
        };

        // Step 6: Save the order.
        Orders.Add(order);

        _logger.LogInformation(
            "Created order {OrderId} for candidate {CandidateNumber}",
            order.Id,
            order.CandidateNumber);

        // Step 7: Return a DTO rather than exposing the entity.
        return new OrderResponse
        {
            Id = order.Id,
            CandidateNumber = order.CandidateNumber,
            SubjectIds = order.SubjectIds,
            Status = order.Status
        };
    }
}
