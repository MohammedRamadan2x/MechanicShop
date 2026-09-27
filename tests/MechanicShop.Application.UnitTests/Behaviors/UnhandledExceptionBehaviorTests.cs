using MechanicShop.Application.Common.Behaviors;

using Microsoft.Extensions.Logging;

using NSubstitute;

using MediatR;

using Xunit;

public class UnhandledExceptionBehaviorTests
{
    private readonly ILogger<DummyRequest> _logger = 
        Substitute.For<ILogger<DummyRequest>>();

    private readonly UnhandledExceptionBehaviour<DummyRequest, string> _sut;

    public UnhandledExceptionBehaviorTests()
    {
        _sut = new UnhandledExceptionBehaviour<DummyRequest, string>(_logger);
    }

    [Fact]
    public async Task Handle_InvokesNextAndReturnsResult_WhenNoException()
    {
        // Arrange
        var request = new DummyRequest();
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns("OK");

        // Act
        var result = await _sut.Handle(request, next, CancellationToken.None);

        // Assert
        await next.Received(1).Invoke();
        Assert.Equal("OK", result);
    }

    [Fact]
    public async Task Handle_LogsErrorAndRethrows_WhenExceptionThrown()
    {
        // Arrange
        var request = new DummyRequest();
        var exception = new InvalidOperationException("test failure");

        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next.Invoke().Returns<Task<string>>(_ => throw exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(request, next, CancellationToken.None));

        Assert.Equal(exception, ex);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Unhandled Exception")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    public class DummyRequest;
}