namespace BO.DTOs;

public record CreateOrderItemRequest(int ProductId, int Quantity, decimal Price);

public record CreateOrderRequest(
    string ShippingAddress,
    string Phone,
    string? FullName,
    string? Email,
    string? City,
    string? PostalCode,
    string PaymentMethod,
    List<CreateOrderItemRequest> Items);

public record CreateOrderResponse(int OrderId, string OrderInvoiceNumber, decimal TotalAmount);

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal Price, string ImageUrl);

public record OrderDto(
    int Id,
    string OrderInvoiceNumber,
    decimal TotalAmount,
    string Status,
    string? ShippingAddress,
    string? Phone,
    DateTime CreatedAt,
    List<OrderItemDto> Items);
