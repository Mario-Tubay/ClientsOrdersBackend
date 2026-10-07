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
public class ClientsController : ControllerBase
{
    private readonly ClientOrderDbContext _context;

    public ClientsController(ClientOrderDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll([FromQuery] string? search, [FromQuery] string? status)
    {
        var query = _context.Clients.Include(c => c.Orders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(s) ||
                c.DocumentNumber.ToLower().Contains(s) ||
                c.Email.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(status) && status.ToLower() != "all")
        {
            if (status.ToLower() == "active")
            {
                query = query.Where(c => c.IsActive);
            }
            else if (status.ToLower() == "inactive")
            {
                query = query.Where(c => !c.IsActive);
            }
        }

        var clients = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClientDto
            {
                Id = c.Id,
                DocumentNumber = c.DocumentNumber,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                TotalOrders = c.Orders.Count
            })
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado." });
        }

        return Ok(new ClientDto
        {
            Id = client.Id,
            DocumentNumber = client.DocumentNumber,
            FullName = client.FullName,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            TotalOrders = client.Orders.Count
        });
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request)
    {
        if (await _context.Clients.AnyAsync(c => c.DocumentNumber == request.DocumentNumber.Trim()))
        {
            return Conflict(new { message = "Ya existe un cliente con esta identificación o RUC." });
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            DocumentNumber = request.DocumentNumber.Trim(),
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            Address = request.Address.Trim(),
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var result = new ClientDto
        {
            Id = client.Id,
            DocumentNumber = client.DocumentNumber,
            FullName = client.FullName,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            TotalOrders = 0
        };

        return CreatedAtAction(nameof(GetById), new { id = client.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var client = await _context.Clients
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado." });
        }

        if (await _context.Clients.AnyAsync(c => c.DocumentNumber == request.DocumentNumber.Trim() && c.Id != id))
        {
            return Conflict(new { message = "Ya existe otro cliente con esta identificación o RUC." });
        }

        client.DocumentNumber = request.DocumentNumber.Trim();
        client.FullName = request.FullName.Trim();
        client.Email = request.Email.Trim();
        client.Phone = request.Phone.Trim();
        client.Address = request.Address.Trim();
        client.IsActive = request.IsActive;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ClientDto
        {
            Id = client.Id,
            DocumentNumber = client.DocumentNumber,
            FullName = client.FullName,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            IsActive = client.IsActive,
            CreatedAt = client.CreatedAt,
            TotalOrders = client.Orders.Count
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound(new { message = "Cliente no encontrado." });
        }

        if (client.Orders.Any())
        {
            return BadRequest(new { message = "No se puede eliminar el cliente porque tiene pedidos asociados. Puedes desactivarlo en su lugar." });
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
