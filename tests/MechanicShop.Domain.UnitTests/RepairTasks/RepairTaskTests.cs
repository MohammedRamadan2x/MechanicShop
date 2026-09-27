using MechanicShop.Domain.RepairTasks.Enums;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Domain.RepairTasks;

using Xunit;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class RepairTaskTests
{
    [Fact]
    public void CreateRepairTask_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string name = "SomeTask";
        const decimal laborCost = 100m;
        const decimal partCost = 50m;
        const int partQuantity = 1;

        const RepairDurationInMinutes estimatedDurationInMin = 
            RepairDurationInMinutes.Min30;

        List<Part> parts = [PartFactory.CreatePart(
            cost: partCost, 
            quantity: partQuantity).Value];

        const decimal totalCost = (partCost * partQuantity) + laborCost;

        var result = RepairTask.Create(
            id,
            name,
            laborCost,
            estimatedDurationInMin,
            parts);

        Assert.True(result.IsSuccess);

        var task = result.Value;

        Assert.Equal(id, task.Id);
        Assert.Equal(name, task.Name);
        Assert.Equal(laborCost, task.LaborCost);
        Assert.Equal(estimatedDurationInMin, task.EstimatedDurationInMins);
        Assert.Single(task.Parts);
        Assert.Equal(totalCost, task.TotalCost);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WhenEmptyName()
    {
        var result = RepairTaskFactory.CreateRepairTask(name: " ");

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void CreateRepairTask_ShouldFail_WhenInvalidLaborCost(
        decimal invalidLaborCost)
    {
        var result = RepairTaskFactory.CreateRepairTask(laborCost: invalidLaborCost);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.InvalidLaborCost.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WhenInvalidDuration()
    {
        var result = RepairTaskFactory.CreateRepairTask(
            repairDurationInMinutes: (RepairDurationInMinutes)999);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.InvalidDuration.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WhenPartsNull()
    {
        var result = RepairTask.Create(
            Guid.NewGuid(),
            "SomeTask",
            100,
            RepairDurationInMinutes.Min30,
            null!);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.PartsRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WhenPartsEmpty()
    {
        var result = RepairTaskFactory.CreateRepairTask(parts: []);

        Assert.True(result.IsFailure);
        Assert.Equal(RepairTaskErrors.PartsRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void CreateRepairTask_ShouldFail_WhenPartIsNull()
    {
        var validPart = PartFactory.CreatePart().Value;

        var result = RepairTaskFactory.CreateRepairTask(
            parts: [validPart, null!]);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.InvalidPart.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData("Brake Pad", "Brake Pad")]
    [InlineData("Brake Pad", "BRAKE PAD")]
    [InlineData("Brake Pad", "  Brake Pad  ")]
    public void CreateRepairTask_ShouldFail_WhenDuplicatePartNames(
        string name1,
        string name2)
    {
        var part1 = PartFactory.CreatePart(name: name1).Value;
        var part2 = PartFactory.CreatePart(name: name2).Value;

        var result = RepairTaskFactory.CreateRepairTask(
            parts: [part1, part2]);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.DuplicatePartName.Code, result.TopError.Code);
    }

    [Fact]
    public void UpsertParts_AddsNewPart_WhenNotExisting()
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;
        var incoming = PartFactory.CreatePart().Value;

        var result = task.UpsertParts([incoming]);

        Assert.True(result.IsSuccess);
        Assert.Contains(incoming, task.Parts);
    }

    [Fact]
    public void UpsertParts_UpdatesExistingPart_WhenExisting()
    {
        var id = Guid.NewGuid();

        var original = PartFactory.CreatePart(
            id: id, 
            name: "Old", 
            cost: 10, 
            quantity: 2).Value;

        var task = RepairTaskFactory.CreateRepairTask(parts: [original]).Value;

        var incoming = PartFactory.CreatePart(
            id: id, 
            name: "New", 
            cost: 20, 
            quantity: 5).Value;

        var result = task.UpsertParts([incoming]);

        Assert.True(result.IsSuccess);

        var updated = task.Parts.First(p => p.Id == id);

        Assert.Equal("New", updated.Name);
        Assert.Equal(20m, updated.Cost);
        Assert.Equal(5, updated.Quantity);
    }

    [Fact]
    public void UpsertParts_RemovesMissingParts()
    {
        var keep = PartFactory.CreatePart(name: "Brake Pad").Value;
        var remove = PartFactory.CreatePart(name: "Oil Filter").Value;

        var task = RepairTaskFactory.CreateRepairTask(parts: [keep, remove]).Value;

        var result = task.UpsertParts([keep]);

        Assert.True(result.IsSuccess);
        Assert.Single(task.Parts);
        Assert.Contains(keep, task.Parts);
        Assert.DoesNotContain(remove, task.Parts);
    }

    [Fact]
    public void UpdateRepairTask_ShouldSucceed_WithValidData()
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;

        var result = task.Update("Valid", 123m, RepairDurationInMinutes.Min30);

        Assert.True(result.IsSuccess);

        Assert.Equal("Valid", task.Name);
        Assert.Equal(123m, task.LaborCost);
        Assert.Equal(RepairDurationInMinutes.Min30, task.EstimatedDurationInMins);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateRepairTask_ShouldFail_WhenNameIsEmptyOrWhitespace(string name)
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;

        var result = task.Update(name, 1, RepairDurationInMinutes.Min30);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.NameRequired.Code, result.TopError.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10001)]
    public void UpdateRepairTask_ShouldFail_WhenInvalidLaborCost(decimal laboCost)
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;

        var result = task.Update("Name", laboCost, RepairDurationInMinutes.Min30);

        Assert.True(result.IsFailure);

        Assert.Equal(RepairTaskErrors.InvalidLaborCost.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateRepairTask_ShouldFail_WhenInvalidDuration()
    {
        var task = RepairTaskFactory.CreateRepairTask().Value;

        var result = task.Update("Name", 1m, (RepairDurationInMinutes)999);

        Assert.False(result.IsSuccess);

        Assert.Equal(RepairTaskErrors.InvalidDuration.Code, result.TopError.Code);
    }
}