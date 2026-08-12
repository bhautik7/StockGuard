using Microsoft.Extensions.Configuration;
using StockGuard.Infrastructure.Identity;

namespace StockGuard.IntegrationTests;

public static class TestTokenHelper
{
    public static string GenerateToken(string role)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "StockGuard-Super-Secret-Key-For-Development-Only-Change-In-Production-2026",
                ["Jwt:Issuer"] = "StockGuard.Api",
                ["Jwt:Audience"] = "StockGuard.Client",
                ["Jwt:ExpiryMinutes"] = "60"
            })
            .Build();

        var generator = new JwtTokenGenerator(config);
        var fakeUser = new ApplicationUser { Id = Guid.NewGuid(), Email = "test@stockguard.com", FullName = "Test User" };
        return generator.GenerateToken(fakeUser, new List<string> { role });
    }
}