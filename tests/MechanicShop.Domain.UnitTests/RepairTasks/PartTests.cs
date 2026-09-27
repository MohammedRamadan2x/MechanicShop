using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Domain.Common.Results;

using Xunit;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class PartTests
{
    [Fact]
    public void CreatePart_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string name = "Brake Pad";
        const decimal cost = 100m;
        const int quantity = 2;

        var result = PartFactory.CreatePart(id, name, cost, quantity);

        Assert.True(result.IsSuccess);

        var part = result.Value;

        Assert.IsType<Part>(part);
        Assert.Equal(id, part.Id);
        Assert.Equal(name, part.Name);
        Assert.Equal(cost, part.Cost);
        Assert.Equal(quantity, part.Quantity);
    }

    [Fact]
    public void CreatePart_ShouldFail_WhenInvalidName()
    {
        var result = PartFactory.CreatePart(name: " ");

        Assert.True(result.IsFailure);

        Assert.Equal(PartErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void CreatePart_ShouldFail_WhenInvalidCost(decimal invalidCost)
    {
        var result = PartFactory.CreatePart(cost: invalidCost);

        Assert.True(result.IsFailure);

        Assert.Equal(PartErrors.InvalidCost.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Create_ShouldFail_WhenInvalidQuantity(int invalidQuantity)
    {
        var result = PartFactory.CreatePart(quantity: invalidQuantity);

        Assert.True(result.IsFailure);

        Assert.Equal(PartErrors.InvalidQuantity.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdatePart_ShouldSucceed_WithValidData()
    {
        var part = PartFactory.CreatePart().Value;

        const string name = "Brake Disc";
        const decimal cost = 200m;
        const int quantity = 3;

        var result = part.Update(name, cost, quantity);

        Assert.True(result.IsSuccess);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(name, part.Name);
        Assert.Equal(cost, part.Cost);
        Assert.Equal(quantity, part.Quantity);
    }

    [Fact]
    public void UpdatePart_ShouldFail_WhenInvalidName()
    {
        var part = PartFactory.CreatePart().Value;

        var result = part.Update(" ", 200m, 3);

        Assert.True(result.IsFailure);

        Assert.Equal(PartErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void UpdatePart_ShouldFail_WhenInvalidCost(decimal invalidCost)
    {
        var part = PartFactory.CreatePart().Value;

        var result = part.Update("Brake Disc", invalidCost, 3);

        Assert.True(result.IsFailure);

        Assert.Equal(PartErrors.InvalidCost.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void UpdatePart_ShouldFail_WhenInvalidQuantity(int invalidQuantity)
    {
        var part = PartFactory.CreatePart().Value;

        var result = part.Update("Brake Disc", 200m, invalidQuantity);

        Assert.True(result.IsFailure);

        Assert.Equal(PartErrors.InvalidQuantity.Code, result.TopError.Code);
    }
}