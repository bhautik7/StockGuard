using NetArchTest.Rules;
using Xunit;

namespace StockGuard.ArchitectureTests;

public class DependencyRuleTests
{
    private const string DomainNamespace = "StockGuard.Domain";
    private const string ApplicationNamespace = "StockGuard.Application";
    private const string InfrastructureNamespace = "StockGuard.Infrastructure";

    [Fact]
    public void Domain_Should_Not_DependOn_Application()
    {
        var result = Types.InAssembly(typeof(Domain.Entities.Product).Assembly)
            .That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_Should_Not_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Domain.Entities.Product).Assembly)
            .That().ResideInNamespace(DomainNamespace)
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_Should_Not_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Application.Services.FefoAllocationService).Assembly)
            .That().ResideInNamespace(ApplicationNamespace)
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}