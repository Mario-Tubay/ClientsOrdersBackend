using ClientOrderService.API.Data;
using ClientOrderService.API.DTOs;
using ClientOrderService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientOrderService.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ClientOrderDbContext _context;

    public OrdersController(ClientOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(s) ||
                (o.Client != null && o.Client.FullName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            query = query.Where(o => o.Status.ToLower() == status.ToLower());
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                ClientId = o.ClientId,
                ClientName = o.Client != null ? o.Client.FullName : "Sin Asignar",
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            })
            .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        return Ok(new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ClientId = order.ClientId,
            ClientName = order.Client != null ? order.Client.FullName : "Sin Asignar",
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request)
    {
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null)
        {
            return BadRequest(new { message = "El cliente seleccionado no existe." });
        }

        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(new { message = "El pedido debe contener al menos un producto o servicio." });
        }

        string orderNumber = request.OrderNumber?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            var totalCount = await _context.Orders.CountAsync();
            orderNumber = $"PED-{DateTime.UtcNow.Year}-{(totalCount + 1):D3}";
        }

        if (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber))
        {
            return Conflict(new { message = $"El número de orden {orderNumber} ya existe." });
        }

        var orderId = Guid.NewGuid();
        var items = request.Items.Select(i => new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductName = i.ProductName.Trim(),
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Quantity * i.UnitPrice
        }).ToList();

        var totalAmount = items.Sum(i => i.Subtotal);

        var order = new Order
        {
            Id = orderId,
            OrderNumber = orderNumber,
            ClientId = request.ClientId,
            OrderDate = DateTime.UtcNow,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ClientId = order.ClientId,
            ClientName = client.FullName,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, [FromBody] UpdateOrderRequest request)
    {
        var order = await _context.Orders
            .Include(o => o.Client)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null)
        {
            return BadRequest(new { message = "El cliente seleccionado no existe." });
        }

        _context.OrderItems.RemoveRange(order.Items);

        var newItems = request.Items.Select(i => new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            ProductName = i.ProductName.Trim(),
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Quantity * i.UnitPrice
        }).ToList();

        order.ClientId = request.ClientId;
        order.Status = request.Status.Trim();
        order.Notes = request.Notes?.Trim() ?? string.Empty;
        order.TotalAmount = newItems.Sum(i => i.Subtotal);
        order.UpdatedAt = DateTime.UtcNow;
        order.Items = newItems;

        await _context.SaveChangesAsync();

        return Ok(new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ClientId = order.ClientId,
            ClientName = client.FullName,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Subtotal
            }).ToList()
        });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        order.Status = request.Status.Trim();
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Estado actualizado con éxito.", status = order.Status });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Pedido no encontrado." });
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
