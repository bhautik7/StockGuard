using Microsoft.AspNetCore.Identity;

namespace StockGuard.Infrastructure.Identity;

public static class RoleSeeder
{
    public static readonly string[] AllRoles =
    {
        "Administrator",
        "InventoryManager",
        "PurchasingOfficer",
        "WarehouseEmployee",
        "Auditor"
    };

    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in AllRoles)
        {
            var exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }
}