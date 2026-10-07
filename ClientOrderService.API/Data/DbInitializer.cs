using ClientOrderService.API.Models;

namespace ClientOrderService.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ClientOrderDbContext context)
    {
        if (context.Clients.Any())
        {
            return;
        }

        var client1 = new Client
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "0992345678001",
            FullName = "Importadora Andina S.A.",
            Email = "contacto@andina.com.ec",
            Phone = "+593 4 234 5678",
            Address = "Av. Juan Tanca Marengo Km 4.5",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var client2 = new Client
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "1791234567001",
            FullName = "Logística del Pacífico Cía.",
            Email = "operaciones@pacificolog.com",
            Phone = "+593 2 543 2100",
            Address = "Parque Industrial Sur, Lote 12",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        var client3 = new Client
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "0998765432001",
            FullName = "Distribuidora Guayas",
            Email = "ventas@distguayas.ec",
            Phone = "+593 4 600 7890",
            Address = "Km 14 Vía a Daule",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        var client4 = new Client
        {
            Id = Guid.NewGuid(),
            DocumentNumber = "0991122334001",
            FullName = "Corporación Marítima",
            Email = "gerencia@maritima.com",
            Phone = "+593 4 288 9900",
            Address = "Puerto Marítimo de Guayaquil",
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddMonths(-4)
        };

        await context.Clients.AddRangeAsync(client1, client2, client3, client4);
        await context.SaveChangesAsync();

        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "PED-2026-001",
            ClientId = client1.Id,
            OrderDate = DateTime.UtcNow.AddDays(-1),
            Status = "Completed",
            TotalAmount = 1250.00m,
            Notes = "Trámite aduanero completado con éxito",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Despacho Aduanero FCL",
                    Quantity = 2,
                    UnitPrice = 625.00m,
                    Subtotal = 1250.00m
                }
            }
        };

        var order2 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "PED-2026-002",
            ClientId = client2.Id,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            TotalAmount = 3420.50m,
            Notes = "Pendiente de confirmación de aforo",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Inspección de Contenedor",
                    Quantity = 1,
                    UnitPrice = 3420.50m,
                    Subtotal = 3420.50m
                }
            }
        };

        var order3 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "PED-2026-003",
            ClientId = client3.Id,
            OrderDate = DateTime.UtcNow.AddDays(-2),
            Status = "Completed",
            TotalAmount = 890.00m,
            Notes = "Entrega finalizada en bodega",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Transporte Terrestre",
                    Quantity = 1,
                    UnitPrice = 890.00m,
                    Subtotal = 890.00m
                }
            }
        };

        var order4 = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "PED-2026-004",
            ClientId = client4.Id,
            OrderDate = DateTime.UtcNow.AddDays(-5),
            Status = "Cancelled",
            TotalAmount = 2100.00m,
            Notes = "Cancelado por solicitud del importador",
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Almacenaje Temporal",
                    Quantity = 1,
                    UnitPrice = 2100.00m,
                    Subtotal = 2100.00m
                }
            }
        };

        await context.Orders.AddRangeAsync(order1, order2, order3, order4);
        await context.SaveChangesAsync();
    }
}
