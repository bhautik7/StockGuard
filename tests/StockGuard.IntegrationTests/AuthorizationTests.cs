using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using StockGuard.Api;
using Xunit;

namespace StockGuard.IntegrationTests;

public class AuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
   private readonly TestWebApplicationFactory _factory;

    public AuthorizationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

   
    

    [Fact]
    public async Task CreateProduct_AsAuditor_Returns403Forbidden()
    {
        var client = _factory.CreateClient();
        var token = TestTokenHelper.GenerateToken("Auditor");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/products", new
        {
            sku = "TEST-AUTH-001",
            name = "Test Product",
            description = (string?)null,
            unit = "each",
            reorderLevel = 5,
            categoryId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithoutToken_StillSucceeds()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}