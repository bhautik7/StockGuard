using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockGuard.Domain.Entities;
using StockGuard.Infrastructure.Identity;

namespace StockGuard.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        await SeedUsersAsync(userManager);

        if (await context.Products.AnyAsync())
            return; // already seeded

        var categories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Beverages", Description = "Soft drinks, juices and water" },
            new Category { Id = Guid.NewGuid(), Name = "Snacks", Description = "Chips, crackers and bars" },
            new Category { Id = Guid.NewGuid(), Name = "Dairy", Description = "Milk, cheese and yogurt" },
            new Category { Id = Guid.NewGuid(), Name = "Produce", Description = "Fresh fruit and vegetables" },
            new Category { Id = Guid.NewGuid(), Name = "Frozen Foods", Description = "Frozen meals and ice cream" },
        };
        context.Categories.AddRange(categories);

        var suppliers = new[]
        {
            new Supplier { Id = Guid.NewGuid(), Name = "Northwind Distributors", ContactEmail = "orders@northwind.example", ContactPhone = "555-0101" },
            new Supplier { Id = Guid.NewGuid(), Name = "Fresh Farm Co-op", ContactEmail = "sales@freshfarm.example", ContactPhone = "555-0102" },
            new Supplier { Id = Guid.NewGuid(), Name = "Global Foods Ltd", ContactEmail = "hello@globalfoods.example", ContactPhone = "555-0103" },
        };
        context.Suppliers.AddRange(suppliers);

        var warehouses = new[]
        {
            new Warehouse { Id = Guid.NewGuid(), Name = "Central Distribution Center", Location = "Columbus, OH" },
            new Warehouse { Id = Guid.NewGuid(), Name = "West Coast Hub", Location = "Fresno, CA" },
        };
        context.Warehouses.AddRange(warehouses);

        Guid Cat(string name) => categories.First(c => c.Name == name).Id;

        var products = new[]
        {
            NewProduct("BEV-001", "Sparkling Water 12pk", "each", 30, Cat("Beverages")),
            NewProduct("BEV-002", "Orange Juice 1L", "each", 40, Cat("Beverages")),
            NewProduct("BEV-003", "Cold Brew Coffee 6pk", "each", 20, Cat("Beverages")),
            NewProduct("SNK-001", "Sea Salt Potato Chips", "each", 50, Cat("Snacks")),
            NewProduct("SNK-002", "Trail Mix Bar", "each", 60, Cat("Snacks")),
            NewProduct("SNK-003", "Pretzel Sticks", "each", 45, Cat("Snacks")),
            NewProduct("DRY-001", "Whole Milk 1 Gal", "each", 25, Cat("Dairy")),
            NewProduct("DRY-002", "Greek Yogurt 32oz", "each", 20, Cat("Dairy")),
            NewProduct("DRY-003", "Cheddar Cheese Block", "kg", 15, Cat("Dairy")),
            NewProduct("PRD-001", "Bananas", "kg", 80, Cat("Produce")),
            NewProduct("PRD-002", "Roma Tomatoes", "kg", 50, Cat("Produce")),
            NewProduct("PRD-003", "Baby Spinach 5oz", "each", 35, Cat("Produce")),
            NewProduct("FRZ-001", "Frozen Margherita Pizza", "each", 20, Cat("Frozen Foods")),
            NewProduct("FRZ-002", "Vanilla Ice Cream 1.5qt", "each", 25, Cat("Frozen Foods")),
            NewProduct("FRZ-003", "Frozen Mixed Berries", "kg", 18, Cat("Frozen Foods")),
        };
        context.Products.AddRange(products);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var random = new Random(42);
        var batches = new List<InventoryBatch>();
        var batchSeq = 1;

        foreach (var product in products)
        {
            var primaryWarehouse = warehouses[batchSeq % warehouses.Length];

            // A healthy, freshly stocked batch comfortably above reorder level.
            batches.Add(NewBatch(product, primaryWarehouse, batchSeq++, product.ReorderLevel + random.Next(20, 60), today.AddDays(random.Next(60, 180)), BatchStatus.Available));

            // Roughly a third of products run low on stock to demonstrate reorder alerts.
            if (batchSeq % 3 == 0)
            {
                batches.Add(NewBatch(product, warehouses[batchSeq % warehouses.Length], batchSeq++, Math.Max(1, product.ReorderLevel - random.Next(1, product.ReorderLevel / 2 + 1)), today.AddDays(random.Next(30, 90)), BatchStatus.Available));
            }

            // Roughly a quarter are expiring soon to demonstrate expiry alerts.
            if (batchSeq % 4 == 0)
            {
                batches.Add(NewBatch(product, primaryWarehouse, batchSeq++, random.Next(10, 40), today.AddDays(random.Next(1, 7)), BatchStatus.Available));
            }

            // A couple of quarantined batches.
            if (batchSeq % 7 == 0)
            {
                batches.Add(NewBatch(product, warehouses[batchSeq % warehouses.Length], batchSeq++, random.Next(5, 20), today.AddDays(random.Next(10, 40)), BatchStatus.Quarantined));
            }
        }
        context.InventoryBatches.AddRange(batches);

        await context.SaveChangesAsync();

        Product NewProduct(string sku, string name, string unit, int reorderLevel, Guid categoryId) => new()
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            Name = name,
            Description = $"{name} — supplied for the {sku.Split('-')[0]} line.",
            Unit = unit,
            ReorderLevel = reorderLevel,
            CategoryId = categoryId
        };

        InventoryBatch NewBatch(Product product, Warehouse warehouse, int seq, int quantity, DateOnly expiry, BatchStatus status) => new()
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            WarehouseId = warehouse.Id,
            BatchNumber = $"LOT-{seq:D5}",
            QuantityOnHand = quantity,
            QuantityReserved = 0,
            ExpiryDate = expiry,
            Status = status
        };
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var demoUsers = new[]
        {
            new { Email = "admin@stockguard.dev", FullName = "Ava Admin", Role = "Administrator" },
            new { Email = "manager@stockguard.dev", FullName = "Milo Manager", Role = "InventoryManager" },
            new { Email = "staff@stockguard.dev", FullName = "Sam Staff", Role = "WarehouseEmployee" },
        };

        foreach (var demo in demoUsers)
        {
            if (await userManager.FindByEmailAsync(demo.Email) is not null)
                continue;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = demo.Email,
                Email = demo.Email,
                FullName = demo.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Passw0rd!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, demo.Role);
            }
        }
    }
}
