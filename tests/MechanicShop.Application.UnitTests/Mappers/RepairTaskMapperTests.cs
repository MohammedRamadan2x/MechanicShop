using MechanicShop.Application.Features.RepairTasks.Mappers;

using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Tests.Common.RepairTasks;
using MechanicShop.Domain.RepairTasks;

using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class RepairTaskMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var part = PartFactory.CreatePart(
            cost: 100m,
            quantity: 2).Value;

        var repairTask = RepairTaskFactory.CreateRepairTask(
            name: "Oil Change",
            laborCost: 150m,
            parts: [part]).Value;

        var dto = repairTask.ToDto();

        Assert.Equal(repairTask.Id, dto.RepairTaskId);
        Assert.Equal(repairTask.Name, dto.Name);
        Assert.Equal(repairTask.LaborCost, dto.LaborCost);
        Assert.Equal(repairTask.TotalCost, dto.TotalCost);
        Assert.Equal(
            repairTask.EstimatedDurationInMins,
            dto.EstimatedDurationInMins);

        Assert.Single(dto.Parts);

        var partDto = dto.Parts[0];

        Assert.Equal(part.Id, partDto.PartId);
        Assert.Equal(part.Name, partDto.Name);
        Assert.Equal(part.Cost, partDto.Cost);
        Assert.Equal(part.Quantity, partDto.Quantity);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var part1 = PartFactory.CreatePart(
            name: "Oil Filter",
            cost: 100m,
            quantity: 1).Value;

        var part2 = PartFactory.CreatePart(
            name: "Air Filter",
            cost: 200m,
            quantity: 2).Value;

        var repairTask1 = RepairTaskFactory.CreateRepairTask(
            name: "Oil Change",
            laborCost: 150m,
            parts: [part1]).Value;

        var repairTask2 = RepairTaskFactory.CreateRepairTask(
            name: "Filter Replacement",
            laborCost: 200m,
            parts: [part2]).Value;

        var repairTasks = new List<RepairTask>
        {
            repairTask1,
            repairTask2
        };

        var dtos = repairTasks.ToDtos();

        Assert.Equal(repairTasks.Count, dtos.Count);

        Assert.Equal(repairTask1.Id, dtos[0].RepairTaskId);
        Assert.Equal(repairTask1.Name, dtos[0].Name);
        Assert.Equal(repairTask1.LaborCost, dtos[0].LaborCost);
        Assert.Equal(repairTask1.TotalCost, dtos[0].TotalCost);
        Assert.Equal(
            repairTask1.EstimatedDurationInMins,
            dtos[0].EstimatedDurationInMins);

        Assert.Single(dtos[0].Parts);
        Assert.Equal(part1.Id, dtos[0].Parts[0].PartId);

        Assert.Equal(repairTask2.Id, dtos[1].RepairTaskId);
        Assert.Equal(repairTask2.Name, dtos[1].Name);
        Assert.Equal(repairTask2.LaborCost, dtos[1].LaborCost);
        Assert.Equal(repairTask2.TotalCost, dtos[1].TotalCost);
        Assert.Equal(
            repairTask2.EstimatedDurationInMins,
            dtos[1].EstimatedDurationInMins);

        Assert.Single(dtos[1].Parts);
        Assert.Equal(part2.Id, dtos[1].Parts[0].PartId);
    }

    [Fact]
    public void ToDto_ShouldMapPartCorrectly()
    {
        var part = PartFactory.CreatePart(
            name: "Brake Pad",
            cost: 250m,
            quantity: 2).Value;

        var dto = part.ToDto();

        Assert.Equal(part.Id, dto.PartId);
        Assert.Equal(part.Name, dto.Name);
        Assert.Equal(part.Cost, dto.Cost);
        Assert.Equal(part.Quantity, dto.Quantity);
    }

    [Fact]
    public void ToDtos_ShouldMapPartsListCorrectly()
    {
        var part1 = PartFactory.CreatePart(
            name: "Oil Filter",
            cost: 100m,
            quantity: 1).Value;

        var part2 = PartFactory.CreatePart(
            name: "Air Filter",
            cost: 200m,
            quantity: 2).Value;

        var parts = new List<Part>
        {
            part1,
            part2
        };

        var dtos = parts.ToDtos();

        Assert.Equal(parts.Count, dtos.Count);

        Assert.Equal(part1.Id, dtos[0].PartId);
        Assert.Equal(part1.Name, dtos[0].Name);
        Assert.Equal(part1.Cost, dtos[0].Cost);
        Assert.Equal(part1.Quantity, dtos[0].Quantity);

        Assert.Equal(part2.Id, dtos[1].PartId);
        Assert.Equal(part2.Name, dtos[1].Name);
        Assert.Equal(part2.Cost, dtos[1].Cost);
        Assert.Equal(part2.Quantity, dtos[1].Quantity);
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenRepairTaskIsNull()
    {
        RepairTask repairTask = null!;

        Assert.Throws<ArgumentNullException>(() => repairTask.ToDto());
    }

    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenPartIsNull()
    {
        Part part = null!;

        Assert.Throws<ArgumentNullException>(() => part.ToDto());
    }
}
