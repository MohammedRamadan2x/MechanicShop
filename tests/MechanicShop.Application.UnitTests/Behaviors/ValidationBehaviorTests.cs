using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Tests.Common.WorkOrders;
using MechanicShop.Domain.Common.Results;

using FluentValidation.Results;
using FluentValidation;

using NSubstitute;

using MediatR;

using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<
        CreateWorkOrderCommand, 
        Result<WorkOrderDto>> _sut;

    private readonly IValidator<CreateWorkOrderCommand> _mockValidator;

    private readonly RequestHandlerDelegate<Result<WorkOrderDto>> _mockNextBehavior;

    public ValidationBehaviorTests()
    {
        _mockNextBehavior = Substitute
            .For<RequestHandlerDelegate<Result<WorkOrderDto>>>();

        _mockValidator = Substitute.For<IValidator<CreateWorkOrderCommand>>();

        _sut = new(_mockValidator);
    }

    [Fact]
    public async Task InvokeValidationBehavior_ShouldInvokeNextBehavior_WhenValidatorResultIsValid()
    {
        // Arrange
        var createWorkOrderCommand = WorkOrderCommandFactory
            .CreateCreateWorkOrderCommand();

        var workOrderResponse = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        _mockValidator
            .ValidateAsync(createWorkOrderCommand, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockNextBehavior.Invoke().Returns(workOrderResponse);

        // Act
        var result = await _sut.Handle(
            createWorkOrderCommand, 
            _mockNextBehavior, 
            default);

        // Assert
        Assert.True(result.IsSuccess);
        await _mockNextBehavior.Received(1).Invoke();
        Assert.Equal(workOrderResponse, result.Value);
    }

    [Fact]
    public async Task InvokeValidationBehavior_ShouldReturnListOfErrors_WhenValidatorResultIsNotValid()
    {
        // Arrange
        var createWorkOrderCommand = WorkOrderCommandFactory
            .CreateCreateWorkOrderCommand();

        List<ValidationFailure> validationFailures = 
            [new(propertyName: "property1", errorMessage: "property1 is invalid")];

        _mockValidator
            .ValidateAsync(createWorkOrderCommand, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _sut.Handle(
            createWorkOrderCommand, 
            _mockNextBehavior, 
            default);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("property1", result.TopError.Code);
        Assert.Equal("property1 is invalid", result.TopError.Description);
        await _mockNextBehavior.DidNotReceive().Invoke();
    }

    [Fact]
    public async Task InvokeValidationBehavior_ShouldInvokeNextBehavior_WhenNoValidator()
    {
        // Arrange
        var createWorkOrderCommand = WorkOrderCommandFactory
            .CreateCreateWorkOrderCommand();

        var validationBehavior = new ValidationBehavior<
            CreateWorkOrderCommand, 
            Result<WorkOrderDto>>();

        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var workOrderResponse = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        _mockNextBehavior.Invoke().Returns(workOrderResponse);

        // Act
        var result = await validationBehavior.Handle(
            createWorkOrderCommand, 
            _mockNextBehavior, 
            default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(workOrderResponse, result.Value);
    }
}