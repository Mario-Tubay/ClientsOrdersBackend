using System.ComponentModel.DataAnnotations;

namespace ClientOrderService.API.DTOs;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemRequest
{
    [Required]
    [StringLength(150)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Range(0.01, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}

public class CreateOrderRequest
{
    [StringLength(50)]
    public string? OrderNumber { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    [StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class UpdateOrderRequest
{
    [Required]
    public Guid ClientId { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Pending";

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;

    public List<CreateOrderItemRequest> Items { get; set; } = new();
}

public class UpdateOrderStatusRequest
{
    [Required]
    [StringLength(30)]
    public string Status { get; set; } = string.Empty;
}
